﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;

namespace ModelLearning.Learners {
    class GreedyExtendLearner : Learner {

        private Random ran;
        private SparseHiddenMarkovModel bestHMM;
        private double ll_validation;
        private double best_likelihood_training;
        private int maxExpandAttempts;
        private double finalBWThreshold;
        private int BWiterations;
        private System.IO.StreamWriter intermediateOutputFile;
        private string intermediateOutputFileName;
        private int run = 0;
        private SequenceData testData;
        private double[] solutions;
        //settings
        private int maxStates;

        public GreedyExtendLearner() {
            ran = new Random();
        }

        public void SetIntermediateOutputFile(string fileName) {
            intermediateOutputFileName = fileName;
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
            this.testData = testData;
            intermediateOutputFile = new System.IO.StreamWriter(intermediateOutputFileName + (run++) + ".csv");
            intermediateOutputFile.WriteLine("States, Likelihood training, likelihood validation");

            
            HMMGraph graph = RandomSingleNodeGraph(trainingData.NumSymbols);
            bestHMM = SparseHiddenMarkovModel.FromGraph(graph);
            best_likelihood_training = bestHMM.Evaluate(trainingData.GetAll(), true);
            while (bestHMM.NumberOfStates < maxStates){
                double last_ll_training = best_likelihood_training;
                WriteLine("Number of states: " + bestHMM.NumberOfStates);
                for (int i = 0; i < maxExpandAttempts; i++) { //number of times to try adding a random node
                    graph = bestHMM.ToGraph();
                    RandomlyExtendGraphSparsely(graph);
                    SparseHiddenMarkovModel hmm = SparseHiddenMarkovModel.FromGraph(graph);
                    if (BWiterations > 0)
                        hmm.Learn(trainingData.GetNonempty(), trainingData.GetNonempty(), 0, BWiterations); //Run the BaumWelch algorithm
                    double likelihood = hmm.Evaluate(trainingData.GetAll(), true);
                    if (likelihood > best_likelihood_training) {
                        best_likelihood_training = likelihood;
                        bestHMM = hmm;
                        WriteLine("+");
                        break;
                    }
                    else {
                        WriteLine("-");
                    }
                }
                if (last_ll_training == best_likelihood_training) { //nothing improved, so stop
                    WriteLine("Likelihood not increased for " + maxExpandAttempts + " attempts");
                    break;
                }
                else {
                    ll_validation = bestHMM.Evaluate(validationData.GetAll(), true);
                    OutputIntermediate();
                }
            }
            WriteLine("Final likelihood: " + best_likelihood_training);
            intermediateOutputFile.Close();
        }

        private void OutputIntermediate() {
            //double real_score = PautomacEvaluator.Evaluate(this, testData, solutions);
            intermediateOutputFile.WriteLine(bestHMM.NumberOfStates + ", " + best_likelihood_training + ", " + ll_validation);
            //intermediateOutputFile.WriteLine(bestHMM.NumberOfStates + ", " + real_score);
            intermediateOutputFile.Flush();
            WriteLine("Likelihood increased to: " + ll_validation);
        }

        public void SetSolutions(double[] solutions) {
            this.solutions = solutions;
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
            return "GreedyExtendLearner";
        }

        public override double CalculateProbability(int[] sequence, bool logarithm = false)
        {
            if (sequence.Length == 0)
                return (logarithm ? 0.0 : 1.0);
            else
                return bestHMM.Evaluate(sequence, logarithm);
        }

        public override void Initialise(LearnerParameters parameters, int iteration) {
            maxStates = (int)parameters.AdditionalParameters["maxStates"];
            maxExpandAttempts = (int)parameters.AdditionalParameters["maxExpandAttempts"];
            BWiterations = (int)parameters.AdditionalParameters["BWiterations"];
            finalBWThreshold = (double)parameters.AdditionalParameters["finalBWThreshold"];
        }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter)
        {
            outputWriter.WriteLine("Number of States: {0}", bestHMM.NumberOfStates);
            outputWriter.WriteLine("Number of Symbols: {0}", bestHMM.NumberOfSymbols);
            outputWriter.WriteLine("Max expand attempts: {0}", maxExpandAttempts);
            outputWriter.WriteLine("Final BW threshold: {0}", finalBWThreshold);
            outputWriter.WriteLine("Max states: {0}", maxStates);
            outputWriter.WriteLine("Log likelihood training {0}", best_likelihood_training);
            outputWriter.WriteLine("Log likelihood validation {0}", ll_validation);
            outputWriter.WriteLine();

            //bestHMM.Save(outputWriter, csvWriter);
        }

    }
}
