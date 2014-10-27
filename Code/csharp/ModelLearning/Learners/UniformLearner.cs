using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners {
    class UniformLearner : Learner {

        public double CalculateProbability(int[] sequence) {
            return 1.0;
        }

        public void Learn(SequenceData trainingData, SequenceData testData) {
        }

        public string Name() {
            return "UniformLearner";
        }
    }
}
