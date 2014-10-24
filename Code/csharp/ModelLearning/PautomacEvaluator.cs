using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;

namespace ModelLearning {
    static class PautomacEvaluator {
        public static double Evaluate(double[] guessedProbs, double[] realProbs) {
            double score = 0;
            for (int i = 0; i < guessedProbs.Length; i++) {
                score += realProbs[i] * Math.Log(guessedProbs[i], 2);
            }
            return Math.Pow(2, -score);
        }

        public static double Evaluate(HiddenMarkovModel hmm, SequenceData testSequences, double[] realProbs) {
            double[] guessedProbs = new double[testSequences.Count];
            for (int i = 0; i < testSequences.Count; i++) {
                //calculate probabiliy of each sequence given model

                if (testSequences[i].Length == 0)
                    guessedProbs[i] = 0.5;
                else{
                    ForwardBackwardAlgorithm.Forward(hmm, testSequences[i], out guessedProbs[i]);
                    guessedProbs[i] = Math.Exp(guessedProbs[i]); //convert from log to real number
                }
            }

            guessedProbs = Normalize(guessedProbs);

            return Evaluate(guessedProbs, realProbs);
        }

        private static double[] Normalize(double[] vals){
            double sum = vals.Sum();
            double[] result = new double[vals.Length];
            for (int i = 0; i < vals.Length; i++)
                if (sum == 0)
                    result[i] = 1.0 / vals.Length;
                else
                    result[i] = vals[i] / sum;
            return result;
        }

    }
}
