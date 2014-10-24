using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning {
    interface Learner {

        double CalculateProbability(int[] sequence);

        void Learn(SequenceData trainingData, SequenceData testData);

        string Name();
    }
}
