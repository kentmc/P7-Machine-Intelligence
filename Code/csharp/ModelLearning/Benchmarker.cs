using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ModelLearning {
    static class Benchmarker {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="learners"></param>
        /// <param name="datasets"></param>
        /// <param name="output_file"></param>
        /// <param name="num_runs">Number of times each learner should run on each dataset</param>
        public static void Run(IEnumerable<Learner> learners, IEnumerable<int> datasets, string output_file, int num_runs) {
            StreamWriter file = new System.IO.StreamWriter(output_file);
            WriteColumnNames(file, learners);

            foreach (int i in datasets) {
                file.Write(i + ", " + num_runs + ", ");

                //Load train, and test data
                SequenceData trainData = DataLoader.LoadSequences(@"Data/" + i + ".pautomac.train");
                SequenceData testData = DataLoader.LoadSequences(@"Data/" + i + ".pautomac.test");

                //Load solutions
                double[] solutions = DataLoader.LoadSolutions(@"Data/" + i + ".pautomac_solution.txt");

                BenchmarkLearners(learners, num_runs, trainData, testData, solutions, file);
                
            }
            file.Close();
        }

        private static void WriteColumnNames(StreamWriter file, IEnumerable<Learner> learners) {
            file.Write("Dataset, Runs");
            foreach (Learner l in learners) {
                file.Write(", " + l.Name() + " AvgScore");
                file.Write(", " + l.Name() + " MedianScore");
                file.Write(", " + l.Name() + " AvgTime");
                file.Write(", " + l.Name() + " MedianTime\n");
            }
        }

        private static void BenchmarkLearners(IEnumerable<Learner> learners, int num_runs, SequenceData trainData, SequenceData testData, double[] solutions, StreamWriter file) {

            Dictionary<Learner, double[]> achieved_scores = new Dictionary<Learner, double[]>();
            Dictionary<Learner, double[]> elapsed_times = new Dictionary<Learner, double[]>();

            for (int r = 0; r < num_runs; r++) {
                //Split training data randomly into train and validation sets
                Tuple<SequenceData, SequenceData> split = trainData.RandomSplit(0.6666666);
                trainData = split.Item1;
                SequenceData validationData = split.Item2;

                foreach (Learner learner in learners) {
                    //Track how much time learning takes for the particular Learner
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    learner.Learn(trainData, validationData, testData);
                    sw.Stop();

                    //Save score achieved and time spent
                    elapsed_times[learner][r] = sw.Elapsed.TotalSeconds;
                    achieved_scores[learner][r] = PautomacEvaluator.Evaluate(learner, testData, solutions);
                }
            }

            WriteResultOfRun(learners, achieved_scores, elapsed_times, file);
        }

        private static void WriteResultOfRun(IEnumerable<Learner> learners, Dictionary<Learner, double[]> achieved_scores, Dictionary<Learner, double[]> elapsed_times, StreamWriter file) {
            foreach (Learner learner in learners) {
                file.Write((int)Utilities.Mean(achieved_scores[learner]) + ", ");
                file.Write((int)Utilities.Median(achieved_scores[learner]) + ", ");
                file.Write((int)Utilities.Mean(elapsed_times[learner]) + ", ");
                file.Write((int)Utilities.Median(elapsed_times[learner]));
            }
        }
    }

}

