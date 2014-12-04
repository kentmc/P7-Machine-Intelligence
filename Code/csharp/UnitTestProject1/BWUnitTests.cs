using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelLearning;
using Accord.Statistics.Models.Markov;

namespace UnitTestProject1 {
    [TestClass]
    public class BWUnitTests {
        [TestMethod]
        public void EquivalenceTest() {
            double[] pi = new double[]{0.23, 0.52, 0.25};

            double[,] A = new double[,]{{ 3.0 / 6.0, 2.0 / 6.0, 1.0 / 6.0 },
                                        { 11.0 / 17.0, 2.0 / 17.0, 4.0 / 17.0},
                                        {4.0 / 20.0, 7.0 / 20.0, 9.0 / 20.0}};

            double[,] B = new double[,]{{ 1.0 / 6.0, 1.0 / 6.0, 4.0 / 6.0 },
                                        { 2.0 / 17.0, 3.0 / 17.0, 12.0 / 17.0},
                                        {13.0 / 20.0, 2.0 / 20.0, 5.0 / 20.0}};

            int[][] observations = new int[][]{
                new int[] {0, 0, 1, 2, 1, 1, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 0, 1},
                new int[] {1, 1, 2, 1, 2, 0},
                new int[] {1, 1, 2, 2, 2, 2, 1, 1, 2, 1, 2, 1, 2, 1},
                new int[] {1, 1, 2, 1, 0, 0, 2, 1, 0, 1, 1, 1},
                new int[] {1, 2, 2, 0, 2},
                new int[] {1, 1, 1, 1, 2, 1, 2, 1}
            };

            //Run BW with 20 iterations

            HiddenMarkovModel hmm = new HiddenMarkovModel(Clone(A), Clone(B), Clone(pi));
            hmm.Learn(observations, 20);

            HiddenMarkovModel hmmLM = new HiddenMarkovModel(Clone(A), Clone(B), Clone(pi));
            hmmLM.Learn(observations, 20);

            for (int i = 0; i < hmm.Probabilities.Length; i++)
                Assert.AreEqual(hmm.Probabilities[i], hmmLM.Probabilities[i]);

            for (int i = 0; i < hmm.Transitions.GetLength(0); i++)
                for (int p = 0; p < hmm.Transitions.GetLength(1); p++)
                    Assert.AreEqual(hmm.Transitions[i, p], hmmLM.Transitions[i, p]);

            for (int i = 0; i < hmm.Emissions.GetLength(0); i++)
                for (int p = 0; p < hmm.Emissions.GetLength(1); p++)
                    Assert.AreEqual(hmm.Emissions[i, p], hmmLM.Emissions[i, p]);

            //Run BW with 1 iteration

            hmm = new HiddenMarkovModel(Clone(A), Clone(B), Clone(pi));
            hmm.Learn(observations, 1);

            hmmLM = new HiddenMarkovModel(Clone(A), Clone(B), Clone(pi));
            hmmLM.Learn(observations, 1);

            for (int i = 0; i < hmm.Probabilities.Length; i++)
                Assert.AreEqual(hmm.Probabilities[i], hmmLM.Probabilities[i]);

            for (int i = 0; i < hmm.Transitions.GetLength(0); i++)
                for (int p = 0; p < hmm.Transitions.GetLength(1); p++)
                    Assert.AreEqual(hmm.Transitions[i, p], hmmLM.Transitions[i, p]);

            for (int i = 0; i < hmm.Emissions.GetLength(0); i++)
                for (int p = 0; p < hmm.Emissions.GetLength(1); p++)
                    Assert.AreEqual(hmm.Emissions[i, p], hmmLM.Emissions[i, p]);

            //Run BW with threshold 0.1

            hmm = new HiddenMarkovModel(Clone(A), Clone(B), Clone(pi));
            hmm.Learn(observations, 0.1);

            hmmLM = new HiddenMarkovModel(Clone(A), Clone(B), Clone(pi));
            hmmLM.Learn(observations, 0.1);

            for (int i = 0; i < hmm.Probabilities.Length; i++)
                Assert.AreEqual(hmm.Probabilities[i], hmmLM.Probabilities[i]);

            for (int i = 0; i < hmm.Transitions.GetLength(0); i++)
                for (int p = 0; p < hmm.Transitions.GetLength(1); p++)
                    Assert.AreEqual(hmm.Transitions[i, p], hmmLM.Transitions[i, p]);

            for (int i = 0; i < hmm.Emissions.GetLength(0); i++)
                for (int p = 0; p < hmm.Emissions.GetLength(1); p++)
                    Assert.AreEqual(hmm.Emissions[i, p], hmmLM.Emissions[i, p]);

        }
        private double[] Clone(double[] array) {
            double[] res = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                res[i] = array[i];
            return res;
        }
        private double[,] Clone(double[,] array) {
            double[,] res = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
                for (int p = 0; p < array.GetLength(1); p++)
                    res[i, p] = array[i, p];
            return res;
        }
    
    }
}
