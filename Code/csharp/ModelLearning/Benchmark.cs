using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    class Benchmark
    {
        private Dictionary<Learner, LearnerParameters> learners;

        private DataSet[] dataSets;

        private int numberOfRuns;

        private bool verbose;

        public string Name { get; private set; }

        public Benchmark(string name, IEnumerable<Learner> learners, IEnumerable<int> dataSets, int numberOfRuns, bool verbose)
        {
            Name = name;

            this.dataSets = dataSets.Select(num => new DataSet(num)).ToArray();

            this.numberOfRuns = numberOfRuns;

            this.verbose = verbose;

            this.learners = new Dictionary<Learner, LearnerParameters>();

            foreach (Learner learner in learners)
            {
                LearnerParameters parameters = new LearnerParameters(learner.Name());

                this.learners.Add(learner, parameters);
            }
        }

        public void Run()
        {
            for (int i = 0; i < dataSets.Length; i++)
            {
                BenchmarkDataset(dataSets[i]);
            }
        }

        public void BenchmarkDataset(DataSet dataSet)
        {
            Dictionary<Learner, double> averageLearnerScores = new Dictionary<Learner, double>();
            Dictionary<Learner, double> medianLearnerScores = new Dictionary<Learner, double>();
            Dictionary<Learner, double> averageLearnerRunTimes = new Dictionary<Learner, double>();
            Dictionary<Learner, double> medianLearnerRuntimes = new Dictionary<Learner, double>();

            DirectoryInfo dir = new DirectoryInfo(String.Format("Benchmark_{0}", Name));
            if (!dir.Exists)
            {
                dir.Create();
            }

            dir = new DirectoryInfo(String.Format(@"Benchmark_{0}\DataSet_{1}", Name, dataSet.Number));
            if (!dir.Exists)
            {
                dir.Create();
            }

            using (StreamWriter outputWriter = new StreamWriter(String.Format(@"Benchmark_{0}\DataSet_{1}\SUMMARY.txt", Name, dataSet.Number)))
            {
                using (StreamWriter csvWriter = new StreamWriter(String.Format(@"Benchmark_{0}\DataSet_{1}\SUMMARY.csv", Name, dataSet.Number)))
                {
                    int learnerNamePadding = learners.Keys.Select(l => l.Name().Length).Max();

                    outputWriter.WriteLine("DataSet {0}", dataSet.Number);
                    outputWriter.WriteLine();

                    Learner bestLearner = null;

                    foreach (Learner learner in learners.Keys)
                    {
                        double[] results = BenchmarkLearner(dataSet, learner).ToArray();

                        averageLearnerScores.Add(learner, results[0]);
                        medianLearnerScores.Add(learner, results[1]);
                        averageLearnerRunTimes.Add(learner, results[2]);
                        medianLearnerScores.Add(learner, results[3]);

                        outputWriter.WriteLine("{0}:\t{1:0000.0000000000}\t{2:0000.0000000000}\t{3:000000}\t{4:000000}", bestLearner.Name().PadRight(learnerNamePadding), medianLearnerScores[bestLearner],
                            averageLearnerScores[bestLearner], medianLearnerRuntimes[bestLearner], averageLearnerRunTimes[bestLearner]);

                        if ((bestLearner == null) || (medianLearnerScores[learner] > medianLearnerScores[bestLearner]))
                        {
                            bestLearner = learner;
                        }
                    }

                    outputWriter.WriteLine("BEST");
                    outputWriter.WriteLine();

                    outputWriter.WriteLine("{0}:\t{1:0000.0000000000}\t{2:0000.0000000000}\t{3:000000}\t{4:000000}", bestLearner.Name().PadRight(learnerNamePadding), medianLearnerScores[bestLearner],
                        averageLearnerScores[bestLearner], medianLearnerRuntimes[bestLearner], averageLearnerRunTimes[bestLearner]);
                }
            }
        }

        public IEnumerable<double> BenchmarkLearner(DataSet dataSet, Learner learner)
        {

        }
    }
}
