using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners {
    class UniformLearner : Learner {

        public override double CalculateProbability(int[] sequence, bool logarithm = false) {
            return (logarithm ? 0.0 : 1.0);
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData) {
        }

        public override string Name() {
            return "Uniform Learner";
        }

        public override void Initialise(LearnerParameters parameters, int iteration) { }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter) { }
    }
}
