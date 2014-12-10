using Accord.Statistics.Models.Markov;
using ModelLearning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparseHMMTest
{
    class Program
    {
        private static Random random;

        static HMMGraph CreateGraph(int numberOfSymbols, int numberOfStates, double outDegree)
        {
            HMMGraph graph = new HMMGraph(numberOfSymbols);

            double initialProbabilitySum = 0.0;

            for (int i = 0; i < numberOfStates; i++)
            {
                Node node = new Node();

                node.InitialProbability = random.NextDouble();
                initialProbabilitySum += node.InitialProbability;

                node.Emissions = new Dictionary<int, double>();

                for (int j = 0; j < numberOfSymbols; j++)
                {
                    node.Emissions.Add(j, random.NextDouble());
                }

                double emissionSum = node.Emissions.Values.Sum();

                for (int j = 0; j < numberOfSymbols; j++)
                {
                    node.Emissions[j] /= emissionSum;
                }

                graph.AddNode(node);
            }

            for (int i = 0; i < numberOfStates; i++)
            {
                graph.Nodes[i].InitialProbability /= initialProbabilitySum;

                graph.Nodes[i].Transitions = new Dictionary<Node, double>();

                int outDeg = (int)(((i % 2) == 1) ? Math.Floor(outDegree) : Math.Ceiling(outDegree));

                for (int j = 0; j < outDeg; j++)
                {
                    graph.Nodes[i].Transitions.Add(graph.Nodes[((i + j) % numberOfStates)], random.NextDouble());
                }

                double transitionSum = graph.Nodes[i].Transitions.Values.Sum();

                for (int j = 0; j < outDeg; j++)
                {
                    graph.Nodes[i].Transitions[graph.Nodes[((i + j) % numberOfStates)]] /= transitionSum;
                }
            }

            return graph;
        }

        static void Main(string[] args)
        {
            using (StreamWriter sw = new StreamWriter("results.txt"))
            {
                sw.AutoFlush = true;

                for (int l = 2; l <= 10; l++)
                {
                    random = new Random();

                    int numberOfStates = (l * 10);
                    int numberOfSymbols = 2;
                    double outDegree = l;

                    int numberOfIterations = 20;

                    int trainingSetSize = 2048;
                    int signalLength = 16;

                    int numberOfRuns = 8;

                    //double threshold = 0.001;

                    double sparseProbability = 0;
                    double probability = 0;

                    long sparseTicks = 0;
                    long ticks = 0;

                    double averageSpeedUp = 0;

                    for (int i = 0; i < numberOfRuns; i++)
                    {
                        Console.WriteLine("Run {0}:", (i + 1));

                        HMMGraph graph = CreateGraph(numberOfSymbols, numberOfStates, outDegree);

                        SparseHiddenMarkovModel sparseHMM = SparseHiddenMarkovModel.FromGraph(graph);
                        HiddenMarkovModel hmm = ModelConverter.Graph2HMM(graph);

                        int[] signal = Enumerable.Range(0, 10000).Select(_ => random.Next(numberOfSymbols)).ToArray();
                        int[][] trainingSignals = Enumerable.Range(0, trainingSetSize).Select(_ => Enumerable.Range(0, signalLength).Select(__ => random.Next(numberOfSymbols)).ToArray()).ToArray();

                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        sparseHMM.Learn(trainingSignals, 0.0, numberOfIterations);
                        //int[] sparseHSS = sparseHMM.Viterby(signal, out sparseProbability);
                        sparseTicks = watch.ElapsedTicks;

                        //sparseProbability = sparseHMM.Evaluate(signal, true);

                        watch.Restart();

                        hmm.Learn(trainingSignals, numberOfIterations);
                        //int[] hss = hmm.Decode(signal, true, out probability);
                        ticks = watch.ElapsedTicks;

                        //probability = hmm.Evaluate(signal, true);

                        watch.Stop();

                        averageSpeedUp += ((double)ticks / sparseTicks);

                        Console.WriteLine();
                    }

                    averageSpeedUp /= numberOfRuns;

                    sw.WriteLine("{0}:\t{1:00.00000000000000000000}", l, averageSpeedUp);

                    //Console.WriteLine(sparseProbability);
                    //for (int i = 0; i < sparseHSS.Length; i++)
                    //{
                    //    Console.Write(sparseHSS[i]);
                    //}
                    //Console.WriteLine();
                    //Console.WriteLine(sparseTicks);

                    //Console.WriteLine();

                    //Console.WriteLine(probability);
                    //for (int i = 0; i < hss.Length; i++)
                    //{
                    //    Console.Write(hss[i]);
                    //}
                    //Console.WriteLine();
                    //Console.WriteLine(ticks);

                    //Console.WriteLine();
                    //Console.WriteLine(averageSpeedUp);

                    //Console.ReadKey();
                }
            }
        }
    }
}
