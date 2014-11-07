using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning {
    abstract class Learner {

        private bool verbose;

        public void SetVerbosity(bool val) {
            verbose = val;
        }

        public abstract double CalculateProbability(int[] sequence);

        public abstract void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData);

        public abstract string Name();

        protected void WriteLine(string str){
            if (verbose)
                Console.WriteLine(str);
        }
    }
}
