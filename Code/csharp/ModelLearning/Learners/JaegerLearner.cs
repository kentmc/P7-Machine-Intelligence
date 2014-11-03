using Accord.Statistics.Models.Markov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners
{
    class JaegerLearner : Learner
    {
        private Random random;
        private HiddenMarkovModel bestHmm;

        private double epsilon;

        //settings
        private int maxStates;
        private double baumwelchThreshold;


        public JaegerLearner(int maxStates, double baumwelchThreshold, double precision)
        {
            this.maxStates = maxStates;
            this.baumwelchThreshold = baumwelchThreshold;
            random = new Random();

            epsilon = (1 - precision);
        }

        private HMMGraph Random2NodeGraph(int num_symbols) {
            HMMGraph g = new HMMGraph(num_symbols);
            Node n1 = new Node();
            Node n2 = new Node();
            n1.SetTransition(n2, random.NextDouble());
            n2.SetTransition(n1, random.NextDouble());
            for (int i = 0; i < num_symbols; i++) {
                n1.SetEmission(i, random.NextDouble());
                n2.SetEmission(i, random.NextDouble());
            }
            n1.InitialProbability = random.NextDouble();
            n2.InitialProbability = random.NextDouble();
            g.AddNode(n1);
            g.AddNode(n2);
            g.Normalize();
            return g;
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

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            HMMGraph graph = Random2NodeGraph(trainingData.NumSymbols);
            bestHmm = ModelConverter.Graph2HMM(graph);
            //best_likelihood = LogLikelihood(best_hmm, trainingData);

            bestHmm.Learn(trainingData.GetNonempty(), baumwelchThreshold);

            while (bestHmm.States < maxStates)
            {
                WriteLine("Taking one more iteration");

                //HMMGraph old_graph = graph; //for backup if we fail to improve it
                graph = ModelConverter.HMM2Graph(bestHmm);

                Dictionary<int, double> nodePerformance = new Dictionary<int, double>();
                Dictionary<int, int> nodeOccurence = new Dictionary<int, int>();
                double hiddenStateSequenceProbability;
                foreach (int[] signal in trainingData.GetNonempty())
                {
                    int[] hiddenStateSequence = bestHmm.Decode(signal, out hiddenStateSequenceProbability);

                    for (int j = 0; j < hiddenStateSequence.Length;j++)
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
                CutWeakEdges(graph);

                //RandomlyExtendGraph(graph, 1.0 - 1.0 / Math.Log(graph.NumSymbols));
                bestHmm = ModelConverter.Graph2HMM(graph);

                WriteLine("Running BaumWelch");
                bestHmm.Learn(trainingData.GetNonempty(), baumwelchThreshold); //Run the BaumWelch algorithm

                WriteLine("");
                WriteLine("Log Likelihood: {0}" + LogLikelihood(bestHmm, trainingData));
            }
        }

        private void SplitWorstPerformingNode(HMMGraph graph, int node)
        {
            Node splitNode = graph.Nodes[node];
            Node newNode = new Node();

            graph.AddNode(newNode);

            newNode.InitialProbability = splitNode.InitialProbability;

            foreach (Node n in graph.Nodes)
            {
                newNode.SetTransition(n, random.NextDouble());
            }

            foreach (int i in splitNode.Emissions.Keys)
            {
                newNode.SetEmission(i, random.NextDouble());
            }

            foreach (Node n in graph.Nodes)
            {
                if (n.Transitions.ContainsKey(splitNode))
                {
                    n.SetTransition(newNode, n.Transitions[splitNode]);
                }
            }

            graph.Normalize();
        }

        private void CutWeakEdges(HMMGraph graph)
        {
            foreach (Node node in graph.Nodes)
            {
                node.Transitions = node.Transitions.Keys.Where(n => (node.Transitions[n] >= epsilon)).ToDictionary(n => n, n => node.Transitions[n]);
            }

            graph.Normalize();
        }

        public override string Name() {
            return "JaegerLearner";
        }

        public override double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return bestHmm.Evaluate(sequence);
        }
    }
}
