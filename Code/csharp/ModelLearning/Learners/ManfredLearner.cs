using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;

namespace ModelLearning.Learners {
    class ManfredLearner : Learner {

        private Random ran;
        private HiddenMarkovModel best_hmm;
        private double best_likelihood;

        //settings
        private int manfred_iterations, baumwelch_iterations;


        public ManfredLearner(int manfred_iterations, int baumwelch_iterations) {
            this.manfred_iterations = manfred_iterations;
            this.baumwelch_iterations = baumwelch_iterations;
            ran = new Random();     
        }

        private HMMGraph Random2NodeGraph(int num_symbols) {
            HMMGraph g = new HMMGraph(num_symbols);
            Node n1 = new Node();
            Node n2 = new Node();
            n1.SetTransition(n2, ran.NextDouble());
            n2.SetTransition(n1, ran.NextDouble());
            for (int i = 0; i < num_symbols; i++) {
                n1.SetEmission(i, ran.NextDouble());
                n2.SetEmission(i, ran.NextDouble());
            }
            n1.InitialProbability = ran.NextDouble();
            n2.InitialProbability = ran.NextDouble();
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

        public void Learn(SequenceData trainingData, SequenceData testData) {
            HMMGraph graph = Random2NodeGraph(trainingData.NumSymbols);
            best_hmm = ModelConverter.Graph2HMM(graph);
            best_likelihood = LogLikelihood(best_hmm, trainingData);

            while (manfred_iterations > 0) {
                Console.WriteLine("Taking one more iteration");
                manfred_iterations--;
                HMMGraph old_graph = graph; //for backup if we fail to improve it
                graph = ModelConverter.HMM2Graph(best_hmm);
                RandomlyExtendGraph(graph, 1.0 - 1.0 / Math.Log(graph.NumSymbols));
                HiddenMarkovModel hmm = ModelConverter.Graph2HMM(graph);
                Console.WriteLine("Running BaumWelch");
                hmm.Learn(testData.GetAll(), baumwelch_iterations); //Run the BaumWelch algorithm
                double likelihood = LogLikelihood(hmm, trainingData);
                if (likelihood > best_likelihood) {
                    best_likelihood = likelihood;
                    best_hmm = hmm;
                }
            }
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
            new_node.InitialProbability = ran.NextDouble();

            //Set random emission probabilities for the new node
            for (int i = 0; i < g.NumSymbols; i++ )
                new_node.SetEmission(i, ran.NextDouble());

            //Set random transition probabilities from all nodes to the new node and opposite.
            //Graph is kept sparse by removing edges that have probabilities less than threshold
            foreach (Node n in g.Nodes) {
                double new_prob = ran.NextDouble();
                if (new_prob >= threshold)
                    n.SetTransition(new_node, ran.NextDouble());
                new_prob = ran.NextDouble();
                if (new_prob >= threshold)
                    new_node.SetTransition(n, ran.NextDouble());
            }

            //Normalize graph
            g.Normalize();
        }

        public string Name() {
            return "ManfredLearner";
        }

        public double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return best_hmm.Evaluate(sequence);
        }

    }
}
