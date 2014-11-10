using Accord.Statistics.Models.Markov;
using ModelLearning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparseHMMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();

            int numberOfStates = 200;

            HMMGraph graph = new HMMGraph(2);

            double initialProbabilitySum = 0.0;

            for (int i = 0; i < numberOfStates; i++)
            {
                Node node = new Node();

                node.InitialProbability = random.NextDouble();
                initialProbabilitySum += node.InitialProbability;

                node.Emissions = new Dictionary<int, double>();
                node.Emissions.Add(0, random.NextDouble());
                node.Emissions.Add(1, random.NextDouble());

                double emissionSum = node.Emissions.Values.Sum();

                node.Emissions[0] /= emissionSum;
                node.Emissions[1] /= emissionSum;

                graph.AddNode(node);
            }

            for (int i = 0; i < numberOfStates; i++)
            {
                graph.Nodes[i].InitialProbability /= initialProbabilitySum;

                graph.Nodes[i].Transitions = new Dictionary<Node,double>();
                graph.Nodes[i].Transitions.Add(graph.Nodes[((i + 1) % numberOfStates)], random.NextDouble());
                graph.Nodes[i].Transitions.Add(graph.Nodes[((i + 2) % numberOfStates)], random.NextDouble());

                double transitionSum = graph.Nodes[i].Transitions.Values.Sum();

                graph.Nodes[i].Transitions[graph.Nodes[((i + 1) % numberOfStates)]] /= transitionSum;
                graph.Nodes[i].Transitions[graph.Nodes[((i + 2) % numberOfStates)]] /= transitionSum;
            }

            SparseHiddenMarkovModel sparseHMM = SparseHiddenMarkovModel.FromGraph(graph);
            HiddenMarkovModel hmm = ModelConverter.Graph2HMM(graph);

            //int[] signal = new int[] { 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0 };
            int[] signal = Enumerable.Range(0, 10000).Select(_ => random.Next(2)).ToArray();
            int[][] trainingSignals = Enumerable.Range(0, 10).Select(_ => Enumerable.Range(0, 100).Select(__ => random.Next(2)).ToArray()).ToArray();

            double sparseProbability;
            double probability;

            double threshold = 1;

            Stopwatch watch = new Stopwatch();
            watch.Start();

            sparseHMM.Learn(trainingSignals, threshold);
            //int[] sparseHSS = sparseHMM.Viterby(signal, out sparseProbability);
            long sparseTicks = watch.ElapsedTicks;

            sparseProbability = sparseHMM.Evaluate(signal);

            watch.Restart();

            hmm.Learn(trainingSignals, threshold);
            //int[] hss = hmm.Decode(signal, true, out probability);
            long ticks = watch.ElapsedTicks;

            probability = hmm.Evaluate(signal, true);

            watch.Stop();

            Console.WriteLine(sparseProbability);
            //for (int i = 0; i < sparseHSS.Length; i++)
            //{
            //    Console.Write(sparseHSS[i]);
            //}
            //Console.WriteLine();
            Console.WriteLine(sparseTicks);

            Console.WriteLine();

            Console.WriteLine(probability);
            //for (int i = 0; i < hss.Length; i++)
            //{
            //    Console.Write(hss[i]);
            //}
            //Console.WriteLine();
            Console.WriteLine(ticks);

            Console.WriteLine();
            Console.WriteLine((double)ticks / sparseTicks);

            Console.ReadKey();
        }
    }
}
