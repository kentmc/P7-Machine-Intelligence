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
        //private HiddenMarkovModel bestHMM;
        private SparseHiddenMarkovModel bestHMM;

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
        //public double LogLikelihood(HiddenMarkovModel hmm, SequenceData evaluationData) {
        public double LogLikelihood(SparseHiddenMarkovModel hmm, SequenceData evaluationData)
        {
            double loglikelihood = 0;
            for (int i = 0; i < evaluationData.Count; i++)
                loglikelihood += Math.Log(hmm.Evaluate(evaluationData[i]));
            return loglikelihood;
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            //HMMGraph graph = RandomGraph(trainingData.NumSymbols);
            HMMGraph graph;
            //bestHMM = ModelConverter.Graph2HMM(graph);
            bestHMM = SparseHiddenMarkovModel.FromCompleteGraph(trainingData.NumSymbols, trainingData.NumSymbols);

            bestHMM.Learn(trainingData.GetNonempty(), baumwelchThreshold);

            //while (bestHMM.States < maxStates)
            while (bestHMM.NumberOfStates < maxStates)
            {
                WriteLine("Taking one more iteration");

                //graph = ModelConverter.HMM2Graph(bestHMM);
                graph = bestHMM.ToGraph();

                Dictionary<int, double> nodePerformance = new Dictionary<int, double>();
                Dictionary<int, int> nodeOccurence = new Dictionary<int, int>();
                double hiddenStateSequenceProbability;
                foreach (int[] signal in validationData.GetNonempty())
                {
                    //int[] hiddenStateSequence = bestHMM.Decode(signal, out hiddenStateSequenceProbability);
                    int[] hiddenStateSequence = bestHMM.Viterby(signal, out hiddenStateSequenceProbability);

                    for (int j = 0; j < hiddenStateSequence.Length; j++)
                    {
                        if (nodePerformance.ContainsKey(hiddenStateSequence[j]))
                        {
                            //nodePerformance[hiddenStateSequence[j]] += (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.Emissions[hiddenStateSequence[j], signal[j]]));
                            nodePerformance[hiddenStateSequence[j]] += (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.EmissionProbability(hiddenStateSequence[j], signal[j])));
                            nodeOccurence[hiddenStateSequence[j]]++;
                        }
                        else
                        {
                            nodePerformance.Add(hiddenStateSequence[j], (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.EmissionProbability(hiddenStateSequence[j], signal[j]))));
                            nodeOccurence.Add(hiddenStateSequence[j], 1);
                        }
                    }
                }

                foreach (int node in nodeOccurence.Keys)
                {
                    nodePerformance[node] /= nodeOccurence[node];
                }

                int weakPoint = nodePerformance.Keys.Aggregate((a, b) => ((nodePerformance[b] < nodePerformance[a]) ? b : a));
                SplitWorstPerformingNode(graph, weakPoint);

                //bestHMM = ModelConverter.Graph2HMM(graph);
                bestHMM = SparseHiddenMarkovModel.FromGraph(graph);

                WriteLine("Running BaumWelch");
                bestHMM.Learn(trainingData.GetNonempty(), baumwelchThreshold); //Run the BaumWelch algorithm

                WriteLine("");
                WriteLine("Log Likelihood: " + LogLikelihood(bestHMM, trainingData));
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

        public override string Name() {
            return "Strict Jaeger Learner";
        }

        public override double CalculateProbability(int[] sequence, bool logarithm = false)
        {
            if (sequence.Length == 0)
                return (logarithm ? 0.0 : 1.0);
            else
                return bestHMM.Evaluate(sequence, logarithm);
        }

        public override void Initialise(LearnerParameters parameters, int iteration)
        {
            throw new NotImplementedException();
        }

        public override void Save(StreamWriter outputWriter, StreamWriter csvWriter)
        {
            bestHMM.Save(outputWriter, csvWriter);
        }
    }
}
