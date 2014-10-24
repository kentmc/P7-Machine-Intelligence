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

        public static double Evaluate(Learner learner, SequenceData testSequences, double[] solutionData) {
            double[] guessedProbs = new double[testSequences.Count];
            for (int i = 0; i < guessedProbs.Length; i++)
                guessedProbs[i] = learner.CalculateProbability(testSequences[i]);
            guessedProbs = Normalize(guessedProbs);
            return Evaluate(guessedProbs, solutionData);
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
