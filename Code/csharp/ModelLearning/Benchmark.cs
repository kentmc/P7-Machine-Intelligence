using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool useTestData;

        public string Name { get; private set; }

        public Benchmark(string name, IEnumerable<Learner> learners, IEnumerable<int> dataSets, int numberOfRuns, bool useTestData = false)
        {
            Name = name;

            this.dataSets = dataSets.Select(num => new DataSet(num)).ToArray();

            this.numberOfRuns = numberOfRuns;

            this.learners = new Dictionary<Learner, LearnerParameters>();

            this.useTestData = useTestData;

            foreach (Learner learner in learners)
            {
                LearnerParameters parameters = new LearnerParameters(learner.Name());

                this.learners.Add(learner, parameters);
            }
        }

        public void Run()
        {
            DirectoryInfo dir = new DirectoryInfo(String.Format("Benchmark_{0}", Name));
            if (!dir.Exists)
            {
                dir.Create();
            }

            for (int i = 0; i < dataSets.Length; i++)
            {
                BenchmarkDataset(dataSets[i]);
            }
        }

        public void BenchmarkDataset(DataSet dataSet)
        {
            Console.WriteLine("Benchmarking DataSet {0}...", dataSet.Number);

            Dictionary<Learner, double> averageLearnerScores = new Dictionary<Learner, double>();
            Dictionary<Learner, double> medianLearnerScores = new Dictionary<Learner, double>();
            Dictionary<Learner, double> averageLearnerRuntimes = new Dictionary<Learner, double>();
            Dictionary<Learner, double> medianLearnerRuntimes = new Dictionary<Learner, double>();

            DirectoryInfo dir = new DirectoryInfo(String.Format(@"Benchmark_{0}/DataSet_{1}", Name, dataSet.Number));
            if (!dir.Exists)
            {
                dir.Create();
            }

            using (StreamWriter outputWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/SUMMARY.txt", Name, dataSet.Number)),
                                csvWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/SUMMARY.csv", Name, dataSet.Number)))
            {
                int learnerNamePadding = learners.Keys.Select(l => l.Name().Length).Max();

                outputWriter.WriteLine("DataSet {0}", dataSet.Number);
                outputWriter.WriteLine();

                csvWriter.WriteLine("Learner,Median Score,Average Score,Median Time,Average Time,");

                Learner bestLearner = null;

                foreach (Learner learner in learners.Keys)
                {
                    dir = new DirectoryInfo(String.Format(@"Benchmark_{0}/DataSet_{1}/Models_{2}", Name, dataSet.Number, learner.Name().ToLowerInvariant().Replace(' ', '_')));
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }

                    double[] results = BenchmarkLearner(dataSet, learner).ToArray();

                    averageLearnerScores.Add(learner, results[0]);
                    medianLearnerScores.Add(learner, results[1]);
                    averageLearnerRuntimes.Add(learner, results[2]);
                    medianLearnerRuntimes.Add(learner, results[3]);

                    outputWriter.WriteLine("{0}:\t{1:00000000.0000000000}\t{2:0000.0000000000}\t{3:000000}\t{4:000000}", learner.Name().PadRight(learnerNamePadding), medianLearnerScores[learner],
                        averageLearnerScores[learner], medianLearnerRuntimes[learner], averageLearnerRuntimes[learner]);

                    csvWriter.WriteLine("{0},{1},{2},{3},{4},", learner.Name(), medianLearnerScores[learner], averageLearnerScores[learner], medianLearnerRuntimes[learner],
                        averageLearnerRuntimes[learner]);

                    if ((bestLearner == null) || (medianLearnerScores[learner] < medianLearnerScores[bestLearner]))
                    {
                        bestLearner = learner;
                    }
                }

                outputWriter.WriteLine();

                outputWriter.WriteLine("BEST");
                outputWriter.WriteLine();

                outputWriter.WriteLine("{0}:\t{1:00000000.0000000000}\t{2:0000.0000000000}\t{3:000000}\t{4:000000}", bestLearner.Name().PadRight(learnerNamePadding), medianLearnerScores[bestLearner],
                    averageLearnerScores[bestLearner], medianLearnerRuntimes[bestLearner], averageLearnerRuntimes[bestLearner]);
            }
        }

        public IEnumerable<double> BenchmarkLearner(DataSet dataSet, Learner learner)
        {
            Console.WriteLine("Benchmarking Learner {0}...", learner.Name());

            Dictionary<int, double> parameterAverageScores = new Dictionary<int, double>();
            Dictionary<int, double> parameterMedianScores = new Dictionary<int, double>();
            Dictionary<int, double> parameterAverageRuntimes = new Dictionary<int, double>();
            Dictionary<int, double> parameterMedianRuntimes = new Dictionary<int, double>();

            int bestIteration = 0;

            LearnerParameters parameters = learners[learner];

            using (StreamWriter outputWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/{2}.txt", Name, dataSet.Number, learner.Name().ToLowerInvariant().Replace(' ', '_'))),
                                csvSummaryWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/{2}_SUMMARY.csv", Name, dataSet.Number, learner.Name().ToLowerInvariant().Replace(' ', '_'))),
                                csvResultWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/{2}_RESULTS.csv", Name, dataSet.Number, learner.Name().ToLowerInvariant().Replace(' ', '_'))))
            {
                outputWriter.WriteLine("DataSet {0}", dataSet.Number);
                outputWriter.WriteLine("Learner: {0}", learner.Name());
                outputWriter.WriteLine();

                csvSummaryWriter.WriteLine("Iteration,Median Score,Average Score,Median Time,Average Time,");
                csvResultWriter.WriteLine("Iteration,Run,Score,Time,Ticks,");

                for (int i = 0; ((parameters.Minimum + (i * parameters.StepSize)) <= parameters.Maximum); i++)
                {
                    Console.WriteLine("Benchmarking Model with {0}...", IterationName(parameters, i));

                    outputWriter.WriteLine("{0}:", IterationName(parameters, i));
                    outputWriter.WriteLine();

                    learner.Initialise(parameters, i);

                    double[] results = RunLearner(dataSet, learner, outputWriter, csvResultWriter, i).ToArray();

                    parameterAverageScores.Add(i, results[0]);
                    parameterMedianScores.Add(i, results[1]);
                    parameterAverageRuntimes.Add(i, results[2]);
                    parameterMedianRuntimes.Add(i, results[3]);

                    if ((bestIteration < 0) || (parameterMedianScores[bestIteration] > parameterMedianScores[i]))
                    {
                        bestIteration = i;
                    }

                    csvSummaryWriter.WriteLine("{0},{1},{2},{3},{4}", i, parameterMedianScores[i], parameterAverageScores[i], parameterMedianRuntimes[i], parameterAverageRuntimes[i]);
                    csvSummaryWriter.Flush();

                    outputWriter.WriteLine();

                    if (parameters.StepSize == 0)
                    {
                        break;
                    }
                }

                outputWriter.WriteLine();

                outputWriter.WriteLine("SUMMARY");
                outputWriter.WriteLine();

                foreach (int iteration in parameterMedianScores.Keys)
                {
                    outputWriter.WriteLine("{0}:\t{1:00000000.0000000000}\t{2:0000.0000000000}\t{3:000000}\t{4:000000}", IterationName(parameters, iteration), parameterMedianScores[iteration],
                        parameterAverageScores[iteration], parameterMedianRuntimes[iteration], parameterAverageRuntimes[iteration]);
                }
                outputWriter.WriteLine();

                outputWriter.WriteLine("BEST");
                outputWriter.WriteLine();

                outputWriter.WriteLine("{0}:\t{1:00000000.0000000000}\t{2:0000.0000000000}\t{3:000000}\t{4:000000}", IterationName(parameters, bestIteration), parameterMedianScores[bestIteration],
                    parameterAverageScores[bestIteration], parameterMedianRuntimes[bestIteration], parameterAverageRuntimes[bestIteration]);
            }

            yield return parameterAverageScores[bestIteration];
            yield return parameterMedianScores[bestIteration];
            yield return parameterAverageRuntimes[bestIteration];
            yield return parameterMedianRuntimes[bestIteration];
        }

        private string IterationName(LearnerParameters parameters, int iteration)
        {
            if (parameters.StepSize == 0)
            {
                return "Static Setting";
            }

            return String.Format("{0}: {1}", parameters.RunningParameterName, (parameters.Minimum + (iteration * parameters.StepSize)).ToString(parameters.RunningParameterDecimal ? "00.000000" : "000"));
        }

        public IEnumerable<double> RunLearner(DataSet dataSet, Learner learner, StreamWriter outputWriter, StreamWriter csvWriter, int iteration)
        {
            double[] runScores = new double[numberOfRuns];
            double[] runTimes = new double[numberOfRuns];
            double[] runTicks = new double[numberOfRuns];

            Stopwatch watch = new Stopwatch();

            for (int i = 0; i < numberOfRuns; i++)
            {
                Console.WriteLine("Run {0}...", (i + 1));

                watch.Reset();

                dataSet.SplitData(2.0 / 3.0, i);

                watch.Start();

                learner.Learn(dataSet.TrainingData, dataSet.ValidationData, dataSet.TestData);

                watch.Stop();

                double score = 0.0;

                if (useTestData)
                {
                    score = PautomacEvaluator.Evaluate(learner, dataSet.TestData, dataSet.SolutionData);
                }
                else
                {
                    foreach (int[] signal in dataSet.ValidationData.GetAll())
                    {
                        score -= learner.CalculateProbability(signal, true);
                    }
                }

                runScores[i] = score;
                runTimes[i] = (watch.ElapsedMilliseconds / 1000.0);
                runTicks[i] = watch.ElapsedTicks;

                outputWriter.WriteLine("Run {0:00}:\t{1:00000000.0000000000}\t{2:000000}\t{3:0000000000000000}", (i + 1), runScores[i], runTimes[i], runTicks[i]);
                outputWriter.Flush();

                csvWriter.WriteLine("{0},{1},{2},{3},{4},", iteration, i, runScores[i], runTimes[i], runTicks[i]);
                csvWriter.Flush();
                
                using (StreamWriter modelWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/Models_{2}/Iter{3}_Run{4}.txt", Name, dataSet.Number, learner.Name().ToLowerInvariant().Replace(' ', '_'), iteration, i)),
                                    modelCSVWriter = new StreamWriter(String.Format(@"Benchmark_{0}/DataSet_{1}/Models_{2}/Iter{3}_Run{4}.csv", Name, dataSet.Number, learner.Name().ToLowerInvariant().Replace(' ', '_'), iteration, i)))
                {
                    modelWriter.WriteLine("DataSet {0}", dataSet.Number);
                    modelWriter.WriteLine("Learner: {0}", learner.Name());
                    modelWriter.WriteLine("{0}: {1:0000.0000000000}", (useTestData ? "PautomaC Score" : "Log Likelihood"), score);

                    modelWriter.WriteLine();

                    learner.Save(modelWriter, modelCSVWriter);
                }
            }

            yield return runScores.Average();
            yield return Median(runScores);
            yield return runTimes.Average();
            yield return Median(runTimes);
        }

        private double Median(IEnumerable<double> collection)
        {
            double[] orderedCollection = collection.OrderBy(i => i).ToArray();

            double median = orderedCollection[(orderedCollection.Length / 2)];

            if ((orderedCollection.Length % 2) == 0)
            {
                median = ((median + orderedCollection[((orderedCollection.Length / 2) - 1)]) / 2);
            }

            return median;
        }
    }
}
