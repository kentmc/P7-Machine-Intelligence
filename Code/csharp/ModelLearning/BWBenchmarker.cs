using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    using Accord.Statistics.Models.Markov;
    using Learners;

    class BWBenchmarker
    {
        private Random random = new Random();

        private int dataset;

        private SequenceData trainData;
        private SequenceData testData;

        private double[] solutionData;

        public BWBenchmarker(int dataset)
        {
            this.dataset = dataset;

            trainData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.train", dataset));
            testData = DataLoader.LoadSequences(String.Format(@"Data/{0}.pautomac.test", dataset));

            solutionData = DataLoader.LoadSolutions(String.Format(@"Data/{0}.pautomac_solution.txt", dataset));
        }

		public void Run(string name, int numberOfRuns, double threshold, int nMin, int nMax, int stepSize)
        {
			string csvFileName = String.Format("BW_Runs:{0}_Tresh:{1}_Minstate:{2}_MaxState:{3}_StepSize:{4}", numberOfRuns, threshold, nMin, nMax, stepSize);

            Dictionary<int, double[]> runScores = new Dictionary<int, double[]>();
            Dictionary<int, double[]> runTimes = new Dictionary<int, double[]>();
            Dictionary<int, double[]> runTicks = new Dictionary<int, double[]>();

            int bestNumberOfStates = 0;
            double bestScore = Double.MaxValue;

            Tuple<SequenceData, SequenceData>[] data = Enumerable.Range(0, numberOfRuns).Select(_ => testData.RandomSplit(2.0 / 3.0)).ToArray();

 
            using (StreamWriter csvSW = new StreamWriter(String.Format("{0}.csv", csvFileName)))
            {
                csvSW.WriteLine("Dataset,Model_States,Score,Time,");
                csvSW.Flush();
                
                for (int n = nMin; n <= nMax; n += stepSize)
                {
                    Console.WriteLine("{0} states...", n);

                    Run(name, n, numberOfRuns, threshold, runScores, runTimes, runTicks, data);

                    if (runScores[n].Average() < bestScore)
                    {
                        bestScore = runScores[n].Average();
                        bestNumberOfStates = n;
                    }

                    int baseNumberOfStates = bestNumberOfStates;
				
                    for (int i = 0; i < numberOfRuns; i++)
                    {
                        csvSW.WriteLine("{0},{1},{2},{3}", dataset, n, runScores[n][i], runTimes[n][i]);
                    }

                
                    csvSW.WriteLine();
                    csvSW.Flush();
                }

        	}
        }

        private void Run(string name, int numberOfStates, int numberOfRuns, double threshold, Dictionary<int, double[]> runScores, Dictionary<int, double[]> runTimes, Dictionary<int, double[]> runTicks, Tuple<SequenceData, SequenceData>[] data)
        {
            Stopwatch watch = new Stopwatch();

            runScores.Add(numberOfStates, new double[numberOfRuns]);
            runTimes.Add(numberOfStates, new double[numberOfRuns]);
            runTicks.Add(numberOfStates, new double[numberOfRuns]);

            for (int i = 0; i < numberOfRuns; i++)
            {
                Console.WriteLine("run {0}...", i);

                watch.Reset();

                //Learner learner = new BaumWelchLearner(numberOfStates, threshold);

                double[] initialProbabilities = new double[numberOfStates];

                double sum = 0.0;

                for (int k = 0; k < numberOfStates; k++)
                {
                    initialProbabilities[k] = random.NextDouble();
                    sum += initialProbabilities[k];
                }

                for (int k = 0; k < numberOfStates; k++)
                {
                    initialProbabilities[k] /= sum;
                }

                double[,] transitionMatrix = new double[numberOfStates, numberOfStates];

                for (int k = 0; k < numberOfStates; k++)
                {
                    sum = 0.0;

                    for (int l = 0; l < numberOfStates; l++)
                    {
                        transitionMatrix[k, l] = random.NextDouble();
                        sum += transitionMatrix[k, l];
                    }

                    for (int l = 0; l < numberOfStates; l++)
                    {
                        transitionMatrix[k, l] /= sum;
                    }
                }

                double[,] emissionMatrix = new double[numberOfStates, testData.NumSymbols];

                for (int k = 0; k < numberOfStates; k++)
                {
                    sum = 0.0;

                    for (int l = 0; l < testData.NumSymbols; l++)
                    {
                        emissionMatrix[k, l] = random.NextDouble();
                        sum += emissionMatrix[k, l];
                    }

                    for (int l = 0; l < testData.NumSymbols; l++)
                    {
                        emissionMatrix[k, l] /= sum;
                    }
                }

                HiddenMarkovModel model = new HiddenMarkovModel(transitionMatrix, emissionMatrix, initialProbabilities);

                //learner.Learn(data[i].Item1, data[i].Item2, testData);

                watch.Start();

                model.Learn(data[i].Item1.GetNonempty(), threshold);

                watch.Stop();

                Console.WriteLine();

                //double score = PautomacEvaluator.Evaluate(learner, testData, solutionData);

                sum = 0.0;

                int[][] testSignals = testData.GetAll();
                double[] results = new double[testSignals.Length];

                for (int j = 0; j < testSignals.Length; j++)
                {
                    if (testSignals[j].Length == 0)
                    {
                        results[j] = 1;
                    }
                    else
                    {
                        results[j] = model.Evaluate(testSignals[j]);
                    }

                    sum += results[j];
                }

                for (int j = 0; j < testSignals.Length; j++)
                {
                    results[j] /= sum;
                }

                double score = PautomacEvaluator.Evaluate(results, solutionData);

                runScores[numberOfStates][i] = score;
                runTimes[numberOfStates][i] = (watch.ElapsedMilliseconds / 1000);
                runTicks[numberOfStates][i] = watch.ElapsedTicks;

                DirectoryInfo dir = new DirectoryInfo(String.Format("Models_{0}", name));

                if (!dir.Exists)
                {
                    dir.Create();
                }

                using (StreamWriter sw = new StreamWriter(String.Format(@"Models_{0}\n{1}_r{2}.txt", name, numberOfStates, i)))
                {
                    sw.WriteLine("Number of States: {0}", numberOfStates);
                    sw.WriteLine("Threshold: {0}", threshold);
                    sw.WriteLine("Run Number: {0}", i);
                    sw.WriteLine("PautomaC Score: {0:0.0000000000}", score);
                    sw.WriteLine();

                    sw.WriteLine("Initial Distribution:");
                    for (int k = 0; k < numberOfStates; k++ )
                    {
                        sw.Write("{0:0.0000}\t", model.Probabilities[k]);
                    }
                    sw.WriteLine();
                    sw.WriteLine();

                    sw.WriteLine("Transitions:");
                    for (int k = 0; k < numberOfStates; k++)
                    {
                        for (int l = 0; l < numberOfStates; l++)
                        {
                            sw.Write("{0:0.0000}\t", model.Transitions[k, l]);
                        }

                        sw.WriteLine();
                    }
                    sw.WriteLine();

                    sw.WriteLine("Emissions");
                    for (int k = 0; k < numberOfStates; k++)
                    {
                        for (int l = 0; l < testData.NumSymbols; l++)
                        {
                            sw.Write("{0:0.0000}\t", model.Emissions[k, l]);
                        }

                        sw.WriteLine();
                    }
                }
            }
        }

        private double Median (double[] collection)
        {
            double median = collection[(collection.Length / 2)];

            if ((collection.Length % 2) == 0)
            {
                median = ((median + collection[((collection.Length / 2) - 1)]) / 2);
            }

            return median;
        }
    }
}
