using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;

namespace ModelLearning.Learners {
    class BaumWelchLearner : Learner {

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
            hmm = new HiddenMarkovModel(trainingData.NumSymbols, states);
            hmm.Learn(trainingData.GetNonempty(), tolerance);
        }

        public override string Name() {
            return "Baum Welch Learner";
        }
    }
}
