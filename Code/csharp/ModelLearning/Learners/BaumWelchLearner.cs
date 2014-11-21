using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;
using System.IO;

namespace ModelLearning.Learners {
    class BaumWelchLearner : Learner {

        private Random random = new Random();
        HiddenMarkovModel hmm;
        readonly int states;
        readonly double tolerance;

        public BaumWelchLearner(int states, double tolerance) {
            this.states = states;
            this.tolerance = tolerance;
        }

        public override double CalculateProbability(int[] sequence) {
            if (sequence.Length == 0)
                return 1.0;
            else
                return hmm.Evaluate(sequence);
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData) {
            trainingData.AddSequences(validationData);

            double[] initialProbabilities = new double[states];

            double sum = 0.0;

            for (int k = 0; k < states; k++)
            {
                initialProbabilities[k] = random.NextDouble();
                sum += initialProbabilities[k];
            }

            for (int k = 0; k < states; k++)
            {
                initialProbabilities[k] /= sum;
            }

            double[,] transitionMatrix = new double[states, states];

            for (int k = 0; k < states; k++)
            {
                sum = 0.0;

                for (int l = 0; l < states; l++)
                {
                    transitionMatrix[k, l] = random.NextDouble();
                    sum += transitionMatrix[k, l];
                }

                for (int l = 0; l < states; l++)
                {
                    transitionMatrix[k, l] /= sum;
                }
            }

            double[,] emissionMatrix = new double[states, testData.NumSymbols];

            for (int k = 0; k < states; k++)
            {
                sum = 0.0;

                for (int l = 0; l < testData.NumSymbols; l++)
                {
                    emissionMatrix[k, l] = random.NextDouble();
                    sum += emissionMatrix[k, l];
                }

                for (int l = 0; l < testData.NumSymbols; l++)
                {
                    emissionMatrix[k, l] /= sum;
                }
            }

            //hmm = new HiddenMarkovModel(trainingData.NumSymbols, states);
            hmm = new HiddenMarkovModel(transitionMatrix, emissionMatrix, initialProbabilities);
            hmm.Learn(trainingData.GetNonempty(), tolerance);
        }

        public override string Name() {
            return "Baum Welch Learner";
        }

        public override void Save(StreamWriter outputWriter, StreamWriter csvWriter)
        {
            outputWriter.WriteLine(Name());
            outputWriter.WriteLine("Number of States: {0}", states);
            outputWriter.WriteLine("Number of Symbols: {0}", hmm.Symbols);
            outputWriter.WriteLine("Threshold: {0}", tolerance);
            outputWriter.WriteLine();

            string stateString = String.Join(",", Enumerable.Range(0, states));
            csvWriter.WriteLine("State,{0},", stateString);

            //Initial Distribution
            outputWriter.WriteLine("Initial Distribution:");

            csvWriter.Write("Initial,");

            for (int i = 0; i < states; i++)
            {
                outputWriter.Write("{0:0.0000}\t", hmm.Probabilities[i]);

                csvWriter.Write("{0},", hmm.Probabilities[i]);
            }

            outputWriter.WriteLine();
            outputWriter.WriteLine();

            csvWriter.WriteLine();

            //Transitions
            outputWriter.WriteLine("Transitions:");

            csvWriter.WriteLine("Transitions,{0},", stateString);

            for (int i = 0; i < states; i++)
            {
                csvWriter.Write("{0},", i);

                for (int j = 0; j < states; j++)
                {
                    outputWriter.Write("{0:0.0000}\t", hmm.Transitions[i, j]);

                    csvWriter.Write("{0},", hmm.Transitions[j, i]);
                }

                outputWriter.WriteLine();

                csvWriter.WriteLine();
            }

            outputWriter.WriteLine();

            //Emissions

            outputWriter.WriteLine("Emissions");

            csvWriter.WriteLine("Emissions,{0},", stateString);

            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < hmm.Symbols; j++)
                {
                    outputWriter.Write("{0:0.0000}\t", hmm.Emissions[i, j]);
                }

                outputWriter.WriteLine();
            }

            for (int i = 0; i < hmm.Symbols; i++)
            {
                csvWriter.Write("{0},", i);

                for (int j = 0; j < states; j++)
                {
                    csvWriter.Write("{0},", hmm.Emissions[j, i]);
                }

                csvWriter.WriteLine();
            }
        }
    }
}
