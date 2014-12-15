using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;
using System.Threading;
using System.Globalization;

namespace ModelLearning
{
    class Program
    {

        static void Main(string[] args)
        {
			//Modify local system language, so the program will output dots instead of commas for 1.000th seperation
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";

			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            List<Learner> learners = new List<Learner>() {
                new Learners.UniformLearner(),
                new Learners.BaumWelchLearner(),
                new Learners.GreedyExtendLearner(),
                new Learners.SparseBaumWelchLearner(),
                new Learners.StateSplitterLearner(),
                new Learners.JLearner(),
                new Learners.GGLearner(),
                new Learners.PadawanLearner()
            };

            //Console.WriteLine("Select Dataset:");
            //BWBenchmarker benchmarker = new BWBenchmarker(Int32.Parse(Console.ReadLine()));

            //hardcoded datasets to be run

            //int[] datasetarray = new int[6] {38, 28, 23, 25, 16, 1};

            //Console.WriteLine("Number of Runs:");
            //int numberOfRuns = Int32.Parse(Console.ReadLine());

            //Console.WriteLine("Threshold: 0.01 to whatevz");
            //double thresh = Double.Parse(Console.ReadLine());

            //Console.WriteLine("Minimum Number of States:");
            //int nMin = Int32.Parse(Console.ReadLine());

            //Console.WriteLine("Maximum Number of States:");
            //int nMax = Int32.Parse(Console.ReadLine());

            //Console.WriteLine("Step Size:");
            //int stepSize = Int32.Parse(Console.ReadLine());

            //Console.WriteLine("File Name:");
            //string name = Console.ReadLine();

            //foreach(int dataset in datasetarray){
            //    BWBenchmarker benchmarker = new BWBenchmarker(dataset);
            //    benchmarker.Run(name, numberOfRuns, thresh, nMin, nMax, stepSize);
            //};

            while (true)
            {
                //Select number of runs
                int num_runs = ShowInterfaceSelectNumRuns();
                if (num_runs == -1)
                    continue;

                //select output file
                //string output_file = ShowInterfaceSelectOutputFile();
                string benchmarkName = SelectBenchmarkName();

                //select learners
                List<Learner> selected_learners = ShowInterfaceSelectLearners(learners);
                if (selected_learners == null)
                    continue;

                //select datasets
                List<int> selected_datasets = ShowInterfaceSelectDatasets(48);
                if (selected_datasets == null)
                    continue;

                int trainingSetSize = SelectTrainingSetSize();

                //Select verbosity
                bool verbose = ShowInterfaceSelectYesNo("Show intermediate output from learners?");
                foreach (Learner learner in learners)
                    learner.SetVerbosity(verbose);

                bool useTestData = ShowInterfaceSelectYesNo("Use PautomaC test data?");

                Benchmark benchmark = new Benchmark(benchmarkName, selected_learners, selected_datasets, trainingSetSize, num_runs, useTestData);
                benchmark.Run();

                //Run benchmarker
                //Console.WriteLine("\nStarting benchmarker");
                //Benchmarker.Run(selected_learners, selected_datasets, output_file, num_runs);

                Console.WriteLine("\nBenchmarking has finished with success!");
                if (ShowInterfaceSelectYesNo("Do another benchmark?"))
                    continue;
                else
                    break;
            }
        }

        private static string SelectBenchmarkName()
        {
            DateTime dateTime = DateTime.Now;
            string defaultName = dateTime.ToString("HH-mm_dd-MM-yyyy");

            Console.WriteLine("Select Benchmark Name (Default: '{0}'):", defaultName);
            string name = Console.ReadLine();

            if (String.IsNullOrEmpty(name))
            {
                name = defaultName;
            }

            return name;
        }

        static string ShowInterfaceSelectOutputFile()
        {
            Console.WriteLine("\nEnter name for output file (default: 'benchmark_result.txt')");
            string response = Console.ReadLine();
            return response == "" ? "benchmark_result.txt" : response;
        }

        static int ShowInterfaceSelectNumRuns()
        {
            Console.WriteLine("\nEnter number of runs (default: 1, min: 1, max: 1000)");
            string response = Console.ReadLine();
            int runs;
            if (!Int32.TryParse(response, out runs))
                return -1;
            if (runs <= 0 || runs > 1000)
                return -1;
            else
                return runs;
        }

        static int SelectTrainingSetSize()
        {
            Console.WriteLine("\nSelect the size of the training set: (default: 10000)");
            int trainingSetSize;
            if (!Int32.TryParse(Console.ReadLine(), out trainingSetSize))
                return 10000;
            if (trainingSetSize <= 0)
                return 10000;
            else
                return trainingSetSize;
        }

        static List<int> ShowInterfaceSelectDatasets(int num_datasets)
        {
            List<int> selected_datasets = new List<int>();
            Console.WriteLine("\nChoose datasets [1-" + (num_datasets) + "] (e.g. '1,5' for 1 and 5):");
            string response = Console.ReadLine();
            foreach (string s in response.Split(','))
            {
                int dataset_index;
                if (!Int32.TryParse(s, out dataset_index))
                {
                    Console.WriteLine("Could not parse dataset index: '" + s + "'");
                    return null;
                }
                selected_datasets.Add(dataset_index);
                if (dataset_index <= 0 || dataset_index > num_datasets)
                {
                    Console.WriteLine("Dataset must be within range [1-" + num_datasets + "], received: " + response);
                    return null;
                }
            }
            return selected_datasets;
        }

        /// <summary>
        /// Returns true if user selectes yes or false if no
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static bool ShowInterfaceSelectYesNo(string str)
        {
            Console.WriteLine();
            Console.WriteLine(str + " y/n");
            string response = Console.ReadLine();
            if (response.Length > 0 && response.ToLower()[0] == 'y')
                return true;
            else
                return false;
        }

        static List<Learner> ShowInterfaceSelectLearners(List<Learner> learners)
        {
            List<Learner> selected_learners = new List<Learner>();
            for (int i = 0; i < learners.Count; i++)
            {
                Console.WriteLine(i + ": " + learners[i].Name());
            }
            Console.WriteLine("\nChoose Learners: (e.g. '0,2') for 0 and 2");
            string response = Console.ReadLine();
            foreach (string s in response.Split(','))
            {
                int learner_index;
                if (!Int32.TryParse(s, out learner_index))
                {
                    Console.WriteLine("Could not parse learner index: '" + s + "'");
                    return null;
                }
                if (learner_index < 0 || learner_index >= learners.Count)
                {
                    Console.WriteLine("Learner index must be within range [0-" + learners.Count + "], received " + learner_index);
                    return null;
                }
                selected_learners.Add(learners[learner_index]);
            }
            return selected_learners;
        }
    }
}