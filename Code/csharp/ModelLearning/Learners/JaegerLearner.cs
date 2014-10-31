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
        private HiddenMarkovModel best_hmm;
        private double best_likelihood;

        //settings
        private int jaeger_iterations, baumwelch_iterations;


        public JaegerLearner(int jaeger_iterations, int baumwelch_iterations) {
            this.jaeger_iterations = jaeger_iterations;
            this.baumwelch_iterations = baumwelch_iterations;
            random = new Random();     
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

        public void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            HMMGraph graph = Random2NodeGraph(trainingData.NumSymbols);
            best_hmm = ModelConverter.Graph2HMM(graph);
            best_likelihood = LogLikelihood(best_hmm, trainingData);

            for (int i = 0; i < jaeger_iterations; i++)
            {
                Console.WriteLine("Taking one more iteration");

                HMMGraph old_graph = graph; //for backup if we fail to improve it
                graph = ModelConverter.HMM2Graph(best_hmm);

                Dictionary<int, double> nodePerformance = new Dictionary<int, double>();
                Dictionary<int, int> nodeOccurence = new Dictionary<int, int>();
                double hiddenStateSequenceProbability;
                foreach (int[] signal in trainingData.GetNonempty())
                {
                    int[] hiddenStateSequence = best_hmm.Decode(signal, out hiddenStateSequenceProbability);

                    for (int j = 0; j < hiddenStateSequence.Length;j++)
                    {
                        if (nodePerformance.ContainsKey(hiddenStateSequence[j]))
                        {
                            nodePerformance[hiddenStateSequence[j]] += (Math.Log(hiddenStateSequenceProbability) * Math.Log(best_hmm.Emissions[hiddenStateSequence[j], signal[j]]));
                            nodeOccurence[hiddenStateSequence[j]]++;
                        }
                        else
                        {
                            nodePerformance.Add(hiddenStateSequence[j], (Math.Log(hiddenStateSequenceProbability) * Math.Log(best_hmm.Emissions[hiddenStateSequence[j], signal[j]])));
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

                //RandomlyExtendGraph(graph, 1.0 - 1.0 / Math.Log(graph.NumSymbols));
                HiddenMarkovModel hmm = ModelConverter.Graph2HMM(graph);

                Console.WriteLine("Running BaumWelch");
                hmm.Learn(testData.GetNonempty(), baumwelch_iterations); //Run the BaumWelch algorithm
                double likelihood = LogLikelihood(hmm, trainingData);
                if (likelihood > best_likelihood)
                {
                    best_likelihood = likelihood;
                    best_hmm = hmm;
                }
            }
        }

        private void SplitWorstPerformingNode(HMMGraph graph, int node)
        {
            //int newSize = ((int)Math.Sqrt(best_hmm.Transitions.Length) + 1);

            //double[,] newTransitionMatrix = new double[newSize, newSize];
            //double[] newProbabilities = new double[newSize];

            //for (int i = 0; i < (newSize - 1); i++)
            //{
            //    for (int j = 0; j < (newSize - 1); j++)
            //    {
            //        double value = best_hmm.Transitions[i, j];

            //        if (j == node)
            //        {
            //            value /= 2;
            //        }

            //        newTransitionMatrix[i, j] = value;
            //    }
                
            //    newTransitionMatrix[i, (newSize - 1)] = (best_hmm.Transitions[i, node] / 2);
            //    newTransitionMatrix[(newSize - 1), i] = best_hmm.Transitions[node, i];
            //}

            //newTransitionMatrix[(newSize - 1), (newSize - 1)] = 0;

            //HiddenMarkovModel hmm = new HiddenMarkovModel(newTransitionMatrix, best_hmm.Emissions, best_hmm.Probabilities)

            Node splitNode = graph.Nodes[node];
            Node newNode = new Node();

            graph.AddNode(newNode);

            splitNode.InitialProbability /= 2;
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
                    n.Transitions[splitNode] /= 2;
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
            return "JaegerLearner";
        }

        public double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return best_hmm.Evaluate(sequence);
        }
    }
}
