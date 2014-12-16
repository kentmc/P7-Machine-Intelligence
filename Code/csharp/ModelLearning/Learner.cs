using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModelLearning {
    public abstract class Learner {

        private bool verbose;

        public void SetVerbosity(bool val) {
            verbose = val;
        }

        public abstract double CalculateProbability(int[] sequence, bool logarithm = false);

        public abstract void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData);

        public abstract string Name();

        protected void WriteLine(string str){
            if (verbose)
                Console.WriteLine(str);
        }

        public abstract void Initialise(LearnerParameters parameters, int iteration);

        public abstract void Save(StreamWriter outputWriter, StreamWriter csvWriter);
    }
}
