using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;

namespace ModelLearning.Learners {
    class KentManfredLearner : Learner {

        private Random ran;
        private SparseHiddenMarkovModel bestHMM;
        private double bestLikelihood;
        private int alpha;
        private int beta;

        //settings
        private double threshold;

        public KentManfredLearner() {
            ran = new Random();
        }

        private HMMGraph RandomSingleNodeGraph(int num_symbols) {
            HMMGraph g = new HMMGraph(num_symbols);
            Node n1 = new Node();
            n1.SetTransition(n1, ran.NextDouble());
            for (int i = 0; i < num_symbols; i++) {
                n1.SetEmission(i, ran.NextDouble());
            }
            n1.InitialProbability = ran.NextDouble();
            g.AddNode(n1);
            g.Normalize();
            return g;
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData) {
            HMMGraph graph = RandomSingleNodeGraph(trainingData.NumSymbols);
            bestHMM = SparseHiddenMarkovModel.FromGraph(graph);
            bestLikelihood = bestHMM.Evaluate(trainingData.GetAll(), true);
            double ll_increase = threshold;
            while(ll_increase >= threshold){
                double last_ll = bestLikelihood;
                WriteLine("Number of states: " + bestHMM.NumberOfStates);
                SparseHiddenMarkovModel current_best_hmm = null;
                for (int i = 0; i < alpha; i++) {
                    graph = bestHMM.ToGraph();
                    RandomlyExtendGraphSparsely(graph);
                    SparseHiddenMarkovModel hmm = SparseHiddenMarkovModel.FromGraph(graph);
                    hmm.Learn(testData.GetNonempty(), beta); //Run the BaumWelch algorithm
                    double likelihood = hmm.Evaluate(trainingData.GetAll(), true);
                    if (likelihood > bestLikelihood) {
                        bestLikelihood = likelihood;
                        current_best_hmm = hmm;
                        WriteLine("+");
                    }
                    else {
                        WriteLine("-");
                    }
                }
                if (current_best_hmm != null){
                    bestHMM = current_best_hmm;
                    WriteLine("Likelihood increased to " + bestLikelihood);
                }
                else
                    WriteLine("Likelihood stays the same");
                ll_increase = bestLikelihood - last_ll;
            }
            WriteLine("Runs Baum Welch last time with the right threshold");
            bestHMM.Learn(trainingData.GetNonempty(), threshold);
        }


        /// <summary>
        /// Extends the graph by adding a new node with random transitions, emissions and initial prabability.
        /// The new node will be connected to Log n random nodes (back and forth) with random transition, emission and initial probabilities.
        /// </summary>
        /// <param name="g"></param>
        private void RandomlyExtendGraphSparsely(HMMGraph g) {
            //add a new node
            Node new_node = new Node();
            g.AddNode(new_node);

            //Set random initial probability
            new_node.InitialProbability = ran.NextDouble();

            //Set random emission probabilities for the new node
            for (int i = 0; i < g.NumSymbols; i++ )
                new_node.SetEmission(i, ran.NextDouble());

            //Set random transition probabilities from log n random nodes to the new node (in both directions)
            int out_degree = (int)Math.Ceiling(Math.Log(g.NumNodes));
            List<Node> node_list_copy = g.Nodes.Select(n => n).ToList();
            Utilities.Shuffle(node_list_copy);
            for (int i = 0; i < out_degree; i++) {
                node_list_copy[i].SetTransition(new_node, ran.NextDouble());
                new_node.SetTransition(node_list_copy[i], ran.NextDouble());
            }

            //Normalize graph
            g.Normalize();
        }

        /// <summary>
        /// Shuffle any (I)List with an extension method based on the Fisher-Yates shuffle :
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public void Shuffle<T>(IList<T> list) {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public override string Name() {
            return "KentManfredLearner";
        }

        public override double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return bestHMM.Evaluate(sequence);
        }

        public abstract void Initialise(LearnerParameters parameters, int iteration) {
            alpha = (int)parameters.AdditionalParameters["alpha"];
            beta = (int)parameters.AdditionalParameters["beta"];
            threshold = parameters.MinimumThreshold + iteration * parameters.ThresholdStepSize;
        }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter)
        {
            outputWriter.WriteLine("Number of States: {0}", bestHMM.NumberOfStates);
            outputWriter.WriteLine("Number of Symbols: {0}", bestHMM.NumberOfSymbols);
            outputWriter.WriteLine("Alpha: {0}", alpha);
            outputWriter.WriteLine("Beta: {0}", alpha);
            outputWriter.WriteLine();

            bestHMM.Save(outputWriter, csvWriter);
        }

    }
}
