using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners {
    class UniformLearner : Learner {

        public override double CalculateProbability(int[] sequence) {
            return 1.0;
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData) {
        }

        public override string Name() {
            return "UniformLearner";
        }

        public override void Initialise(LearnerParameters parameters, int iteration) { }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter) { }
    }
}
