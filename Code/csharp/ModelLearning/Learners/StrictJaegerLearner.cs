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
        private int jaeger_iterations;
        private double baumwelch_threshold;

        public StrictJaegerLearner(int jaeger_iterations, double baumwelch_threshold) {
            this.jaeger_iterations = jaeger_iterations;
            this.baumwelch_threshold = baumwelch_threshold;
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

        public void Learn(SequenceData trainingData, SequenceData testData)
        {
            using (StreamWriter sw = new StreamWriter("Strict_Jeager.txt"))
            {
                HMMGraph graph = RandomGraph(trainingData.NumSymbols);
                bestHmm = ModelConverter.Graph2HMM(graph);
                //best_likelihood = LogLikelihood(best_hmm, trainingData);

                bestHmm.Learn(trainingData.GetNonempty(), baumwelch_threshold);

                for (int i = 0; i < jaeger_iterations; i++)
                {
                    Console.WriteLine("Taking one more iteration");

                    //HMMGraph old_graph = graph; //for backup if we fail to improve it
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

                    int numberOfStates = (int)Math.Sqrt(bestHmm.Transitions.Length);

                    sw.WriteLine("Iteration: {0}", (i + 1));
                    sw.WriteLine("Initial:");

                    for (int j = 0; j < numberOfStates; j++)
                    {
                        sw.Write("\t{0}", bestHmm.Probabilities[j]);
                    }

                    sw.WriteLine();

                    sw.WriteLine("Transitions:");

                    for (int j = 0; j < numberOfStates; j++)
                    {
                        for (int k = 0; k < numberOfStates; k++)
                        {
                            sw.Write("\t{0}", bestHmm.Transitions[j, k]);
                        }

                        sw.WriteLine();
                    }

                    sw.WriteLine("Emissions:");

                    for (int j = 0; j < numberOfStates; j++)
                    {
                        for (int k = 0; k < trainingData.NumSymbols; k++)
                        {
                            sw.Write("\t{0}", bestHmm.Emissions[j, k]);
                        }

                        sw.WriteLine();
                    }

                    sw.WriteLine("Weak state: {0}", weakPoint);
                    sw.WriteLine();

                    //RandomlyExtendGraph(graph, 1.0 - 1.0 / Math.Log(graph.NumSymbols));
                    bestHmm = ModelConverter.Graph2HMM(graph);

                    Console.WriteLine("Running BaumWelch");
                    bestHmm.Learn(testData.GetNonempty(), baumwelch_threshold); //Run the BaumWelch algorithm

                    Console.WriteLine();
                    Console.WriteLine("Log Likelyhood: {0}", LogLikelihood(bestHmm, trainingData));
                }
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


        /// <summary>
        /// Extends the graph by adding a new node with random transitions, emissions and initial prabability.
        /// Removes all transitions to or from the node that's below the given threshold
        /// </summary>
        /// <param name="g"></param>
        /// <param name="threshold"></param>
        private void RandomlyExtendGraph(HMMGraph g, double threshold) {
            //add a new node
            Node new_node = new Node();
            g.AddNode(new_node);

            //Set random initial probability
            new_node.InitialProbability = random.NextDouble();

            //Set random emission probabilities for the new node
            for (int i = 0; i < g.NumSymbols; i++ )
                new_node.SetEmission(i, random.NextDouble());

            //Set random transition probabilities from all nodes to the new node and opposite.
            //Graph is kept sparse by removing edges that have probabilities less than threshold
            foreach (Node n in g.Nodes) {
                double new_prob = random.NextDouble();
                if (new_prob >= threshold)
                    n.SetTransition(new_node, random.NextDouble());
                new_prob = random.NextDouble();
                if (new_prob >= threshold)
                    new_node.SetTransition(n, random.NextDouble());
            }

            //Normalize graph
            g.Normalize();
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
