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

        private int trainingSetSize;
        private int validationSetSize;

		public DataSet(int number, int trainingSetSize, int validationSetSize)
        {
            Number = number;

            pautomacTrainingData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.train", Number));
            TestData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.test", Number));

            this.trainingSetSize = Math.Min(trainingSetSize, ((pautomacTrainingData.Count * 2) / 3));
			this.validationSetSize = Math.Min(validationSetSize, (pautomacTrainingData.Count - this.trainingSetSize));

            SolutionData = DataLoader.LoadSolutions(String.Format(@"Data/{0}.pautomac_solution.txt", Number));
        }

        public void SplitData(double ratio, int run)
        {
            //shuffle according to Run and Dataset ID
            Tuple<SequenceData, SequenceData> dataSplit = pautomacTrainingData.RandomSplit(trainingSetSize, validationSetSize, run * 100 + Number);

            TrainingData = dataSplit.Item1;
            ValidationData = dataSplit.Item2;
        }
    }
}
