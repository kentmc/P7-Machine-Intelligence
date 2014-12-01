using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    class DataSet
    {
        public int Number { get; private set; }

        public SequenceData TrainingData { get; private set; }
        public SequenceData ValidationData { get; private set; }
        public SequenceData TestData { get; private set; }

        public double[] SolutionData { get; private set; }

        private SequenceData pautomacTrainingData;

        public DataSet(int number)
        {
            Number = number;

            pautomacTrainingData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.train", Number));
            TestData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.test", Number));

            SolutionData = DataLoader.LoadSolutions(String.Format(@"Data/{0}.pautomac_solution.txt", Number));
        }

        public void SplitData(double ratio, int run)
        {
            //shuffle according to Run and Dataset ID
            Tuple<SequenceData, SequenceData> dataSplit = pautomacTrainingData.RandomSplit(ratio, run * 100 + Number);

            TrainingData = dataSplit.Item1;
            ValidationData = dataSplit.Item2;
        }
    }
}
