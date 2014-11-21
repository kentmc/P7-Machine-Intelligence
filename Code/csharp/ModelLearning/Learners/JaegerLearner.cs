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
        private SparseHiddenMarkovModel bestHMM;
        //private HiddenMarkovModel bestHMM;

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
        public double LogLikelihood(SparseHiddenMarkovModel hmm, SequenceData evaluationData)
        //public double LogLikelihood(HiddenMarkovModel hmm, SequenceData evaluationData)
        {
            double loglikelihood = 0;
            for (int i = 0; i < evaluationData.Count; i++)
                //loglikelihood += Math.Log(hmm.Evaluate(evaluationData[i]));
                loglikelihood += hmm.Evaluate(evaluationData[i], true);
            return loglikelihood;
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            HMMGraph graph;
            //HMMGraph graph = Random2NodeGraph(trainingData.NumSymbols);
            bestHMM = SparseHiddenMarkovModel.FromCompleteGraph(2, trainingData.NumSymbols);
            //bestHMM = ModelConverter.Graph2HMM(graph);

            //bestHMM.Learn(trainingData, baumwelchThreshold);
            bestHMM.Learn(trainingData.GetNonempty(), baumwelchThreshold);

            while (bestHMM.NumberOfStates < maxStates)
            //while (bestHMM.States < maxStates)
            {
                WriteLine("Taking one more iteration");

                graph = bestHMM.ToGraph();
                //graph = ModelConverter.HMM2Graph(bestHMM);

                Dictionary<int, double> nodePerformance = new Dictionary<int, double>();
                Dictionary<int, int> nodeOccurence = new Dictionary<int, int>();
                double hiddenStateSequenceProbability;
                foreach (int[] signal in validationData.GetNonempty())
                {
                    int[] hiddenStateSequence = bestHMM.Viterby(signal, out hiddenStateSequenceProbability);
                    //int[] hiddenStateSequence = bestHMM.Decode(signal, out hiddenStateSequenceProbability);

                    for (int j = 0; j < hiddenStateSequence.Length;j++)
                    {
                        if (nodePerformance.ContainsKey(hiddenStateSequence[j]))
                        {
                            nodePerformance[hiddenStateSequence[j]] += (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.EmissionProbability(hiddenStateSequence[j], signal[j])));
                            //nodePerformance[hiddenStateSequence[j]] += (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.Emissions[hiddenStateSequence[j], signal[j]]));
                            nodeOccurence[hiddenStateSequence[j]]++;
                        }
                        else
                        {
                            nodePerformance.Add(hiddenStateSequence[j], (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.EmissionProbability(hiddenStateSequence[j], signal[j]))));
                            //nodePerformance.Add(hiddenStateSequence[j], (Math.Log(hiddenStateSequenceProbability) + Math.Log(bestHMM.Emissions[hiddenStateSequence[j], signal[j]])));
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
                CutWeakEdges(graph);

                bestHMM = SparseHiddenMarkovModel.FromGraph(graph);
                //bestHMM = ModelConverter.Graph2HMM(graph);

                WriteLine("Running BaumWelch");
                //bestHMM.Learn(trainingData, baumwelchThreshold);
                bestHMM.Learn(trainingData.GetNonempty(), baumwelchThreshold);

                WriteLine("Log Likelihood: " + LogLikelihood(bestHMM, validationData));
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
            return "Jaeger Learner";
        }

        public override double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return bestHMM.Evaluate(sequence);
        }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter)
        {
            bestHMM.Save(outputWriter, csvWriter);
        }
    }
}
