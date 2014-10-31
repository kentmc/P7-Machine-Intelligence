using Accord.Statistics.Models.Markov;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners
{
    class StrictJaegerLearner : Learner
    {
        private Random random;
        private HiddenMarkovModel bestHmm;

        //settings
        private int maxStates;
        private double baumwelchThreshold;

        public StrictJaegerLearner(int maxStates, double baumwelchThreshold) {
            this.maxStates = maxStates;
            this.baumwelchThreshold = baumwelchThreshold;
            random = new Random();     
        }

        private HMMGraph RandomGraph(int num_symbols)
        {
            HMMGraph graph = new HMMGraph(num_symbols);

            graph.Nodes = Enumerable.Range(0, num_symbols).Select(_ => new Node()).ToList();

            foreach (Node node in graph.Nodes)
            {
                node.InitialProbability = random.NextDouble();

                foreach (Node n in graph.Nodes)
                {
                    node.SetTransition(n, random.NextDouble());
                }

                for (int i = 0; i < num_symbols; i++)
                {
                    node.SetEmission(i, random.NextDouble());
                }
            }

            return graph;
        }

        /// <summary>
        /// Returns the loglikelihood that the graph have generated the given sequences
        /// </summary>
        /// <param name="g"></param>
        /// <param name="evaluationData"></param>
        /// <returns></returns>
        public double LogLikelihood(HiddenMarkovModel hmm, SequenceData evaluationData) {
            double loglikelihood = 0;
            for (int i = 0; i < evaluationData.Count; i++)
                loglikelihood += hmm.Evaluate(evaluationData[i], true);
            return loglikelihood;
        }

        public void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            HMMGraph graph = RandomGraph(trainingData.NumSymbols);
            bestHmm = ModelConverter.Graph2HMM(graph);

            bestHmm.Learn(trainingData.GetNonempty(), baumwelchThreshold);

            while (bestHmm.States < maxStates)
            {
                Console.WriteLine("Taking one more iteration");

                HMMGraph old_graph = graph; //for backup if we fail to improve it
                graph = ModelConverter.HMM2Graph(bestHmm);

                Dictionary<int, double> nodePerformance = new Dictionary<int, double>();
                Dictionary<int, int> nodeOccurence = new Dictionary<int, int>();
                double hiddenStateSequenceProbability;
                foreach (int[] signal in trainingData.GetNonempty())
                {
                    int[] hiddenStateSequence = bestHmm.Decode(signal, out hiddenStateSequenceProbability);

                    for (int j = 0; j < hiddenStateSequence.Length; j++)
                    {
                        if (nodePerformance.ContainsKey(hiddenStateSequence[j]))
                        {
                            nodePerformance[hiddenStateSequence[j]] += (Math.Log(hiddenStateSequenceProbability) * Math.Log(bestHmm.Emissions[hiddenStateSequence[j], signal[j]]));
                            nodeOccurence[hiddenStateSequence[j]]++;
                        }
                        else
                        {
                            nodePerformance.Add(hiddenStateSequence[j], (Math.Log(hiddenStateSequenceProbability) * Math.Log(bestHmm.Emissions[hiddenStateSequence[j], signal[j]])));
                            nodeOccurence.Add(hiddenStateSequence[j], 1);
                        }
                    }
                }

                foreach (int node in nodeOccurence.Keys)
                {
                    nodePerformance[node] /= nodeOccurence[node];
                }

                int weakPoint = nodePerformance.Keys.Aggregate((a, b) => ((nodePerformance[b] > nodePerformance[a]) ? b : a));
                SplitWorstPerformingNode(graph, weakPoint);

                bestHmm = ModelConverter.Graph2HMM(graph);

                Console.WriteLine("Running BaumWelch");
                bestHmm.Learn(trainingData.GetNonempty(), baumwelchThreshold); //Run the BaumWelch algorithm

                Console.WriteLine();
                Console.WriteLine("Log Likelihood: {0}", LogLikelihood(bestHmm, trainingData));
            }
        }

        private void SplitWorstPerformingNode(HMMGraph graph, int node)
        {
            Node splitNode = graph.Nodes[node];
            Node newNode = new Node();

            graph.AddNode(newNode);

            //splitNode.InitialProbability /= 2;
            newNode.InitialProbability = splitNode.InitialProbability;

            foreach (Node n in splitNode.Transitions.Keys)
            {
                newNode.SetTransition(n, splitNode.Transitions[n]);
            }

            foreach (int i in splitNode.Emissions.Keys)
            {
                newNode.SetEmission(i, splitNode.Emissions[i]);
            }

            foreach (Node n in graph.Nodes)
            {
                if (n.Transitions.ContainsKey(splitNode))
                {
                    //n.Transitions[splitNode] /= 2;
                    n.SetTransition(newNode, n.Transitions[splitNode]);

                    Node weakestTransition = n.Transitions.Keys.Aggregate((a, b) => ((n.Transitions[b] < n.Transitions[a]) ? b : a));

                    n.Transitions.Remove(weakestTransition);
                }
            }

            graph.Normalize();
        }

        public string Name() {
            return "Strict Jaeger Learner";
        }

        public double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return bestHmm.Evaluate(sequence);
        }
    }
}
