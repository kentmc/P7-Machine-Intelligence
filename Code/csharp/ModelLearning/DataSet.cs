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

        private SequenceData unsortedData;

        public DataSet(int number)
        {
            Number = number;

            unsortedData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.train", Number));
            TestData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.test", Number));

            SolutionData = DataLoader.LoadSolutions(String.Format(@"Data/{0}.pautomac_solution.txt", Number));
        }

        public void SplitData(double ratio)
        {
            Tuple<SequenceData, SequenceData> dataSplit = unsortedData.RandomSplit(ratio);

            TrainingData = dataSplit.Item1;
            ValidationData = dataSplit.Item2;
        }
    }
}
