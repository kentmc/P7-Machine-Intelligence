﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    class SparseHiddenMarkovModel
    {
        private Random random;
        private int _numberOfStates;
        private int _numberOfSymbols;

        private double[] initialDistribution;

        private double[,] transitionProbabilities;
        private Dictionary<int, List<int>> transitionsIn;
        private Dictionary<int, List<int>> transitionsOut;

        private double[,] emissionProbabilities;
        private Dictionary<int, List<int>> emissions;
        private Dictionary<int, List<int>> emittents;

        public double[] InitialDistribution
        {
            get
            {
                return initialDistribution;
            }
        }

        public double[,] TransitionMatrix
        {
            get
            {
                return transitionProbabilities;
            }
        }

        public double[,] EmissionMatrix
        {
            get
            {
                return emissionProbabilities;
            }
        }

        public int NumberOfStates
        {
            get { return _numberOfStates; }
            private set { _numberOfStates = value; }
        }

        public int NumberOfSymbols
        {
            get { return _numberOfSymbols; }
            private set { _numberOfSymbols = value; }
        }

        private SparseHiddenMarkovModel(int numberOfStates, int numberOfSymbols)
        {
            random = new Random();

            NumberOfStates = numberOfStates;
            NumberOfSymbols = numberOfSymbols;

            transitionsIn = Enumerable.Range(0, numberOfStates).ToDictionary(i => i, i => new List<int>());
            transitionsOut = Enumerable.Range(0, numberOfStates).ToDictionary(i => i, i => new List<int>());

            emissions = Enumerable.Range(0, numberOfStates).ToDictionary(i => i, i => new List<int>());
            emittents = Enumerable.Range(0, numberOfSymbols).ToDictionary(i => i, i => new List<int>());
        }

        public static SparseHiddenMarkovModel FromGraph(HMMGraph graph)
        {
            SparseHiddenMarkovModel model = new SparseHiddenMarkovModel(graph.NumNodes, graph.NumSymbols);

            model.initialDistribution = graph.Nodes.Select(n => n.InitialProbability).ToArray();
            model.transitionProbabilities = new double[graph.NumNodes, graph.NumNodes];
            model.emissionProbabilities = new double[graph.NumNodes, graph.NumSymbols];

            for (int i = 0; i < graph.NumNodes; i++)
            {
                foreach(Node node in graph.Nodes[i].Transitions.Keys)
                {
                    int index = graph.Nodes.IndexOf(node);

                    model.transitionProbabilities[i, index] = graph.Nodes[i].Transitions[node];

                    model.transitionsIn[index].Add(i);
                    model.transitionsOut[i].Add(index);
                }
            }

            for (int i = 0; i < graph.NumNodes; i++)
            {
                foreach(int j in graph.Nodes[i].Emissions.Keys)
                {
                    model.emissionProbabilities[i, j] = graph.Nodes[i].Emissions[j];

                    model.emissions[i].Add(j);
                    model.emittents[j].Add(i);
                }
            }

            return model;
        }

        public static SparseHiddenMarkovModel FromCompleteGraph(int numberOfStates, int numberOfSymbols)
        {
            SparseHiddenMarkovModel model = new SparseHiddenMarkovModel(numberOfStates, numberOfSymbols);

            model.initialDistribution = new double[numberOfStates];
            double weight = 0.0;

            for (int i = 0; i < numberOfStates; i++)
            {
                double probability = model.random.NextDouble();
                model.initialDistribution[i] = probability;
                weight += probability;
            }

            for (int i = 0; i < numberOfStates; i++)
            {
                model.initialDistribution[i] /= weight;
            }

            model.transitionProbabilities = new double[numberOfStates, numberOfStates];
            model.transitionsIn = Enumerable.Range(0, numberOfStates).ToDictionary(i => i, i => Enumerable.Range(0, numberOfStates).ToList());
            model.transitionsOut = Enumerable.Range(0, numberOfStates).ToDictionary(i => i, i => Enumerable.Range(0, numberOfStates).ToList());

            for (int i = 0; i < numberOfStates; i++ )
            {
                weight = 0.0;

                for (int j = 0; j < numberOfStates; j++)
                {
                    double probability = model.random.NextDouble();
                    model.transitionProbabilities[i, j] = probability;
                    weight += probability;
                }

                for (int j = 0; j < numberOfStates; j++)
                {
                    model.transitionProbabilities[i, j] /= weight;
                }
            }

            model.emissionProbabilities = new double[numberOfStates, numberOfSymbols];
            model.emissions = Enumerable.Range(0, numberOfStates).ToDictionary(i => i, i => Enumerable.Range(0, numberOfSymbols).ToList());
            model.emittents = Enumerable.Range(0, numberOfSymbols).ToDictionary(i => i, i => Enumerable.Range(0, numberOfStates).ToList());

            for (int i = 0; i < numberOfStates; i++)
            {
                weight = 0.0;

                for (int j = 0; j < numberOfSymbols; j++)
                {
                    double probability = model.random.NextDouble();
                    model.emissionProbabilities[i, j] = probability;
                    weight += probability;
                }

                for (int j = 0; j < numberOfSymbols; j++)
                {
                    model.emissionProbabilities[i, j] /= weight;
                }
            }

            return model;
        }

        public HMMGraph ToGraph()
        {
            HMMGraph graph = new HMMGraph(NumberOfSymbols);

            for (int i = 0; i < NumberOfStates; i++)
            {
                Node node = new Node();

                node.InitialProbability = initialDistribution[i];

                node.Emissions = emissions[i].ToDictionary(j => j, j => emissionProbabilities[i, j]);

                graph.AddNode(node);
            }

            for (int i = 0; i < NumberOfStates; i++)
            {
                graph.Nodes[i].Transitions = transitionsOut[i].ToDictionary(j => graph.Nodes[j], j => transitionProbabilities[i, j]);
            }

            return graph;
        }

        public double InitialProbability(int state)
        {
            return initialDistribution[state];
        }

        public double TransitionProbability(int source, int destination)
        {
            return transitionProbabilities[source, destination];
        }

        public double EmissionProbability(int state, int symbol)
        {
            return emissionProbabilities[state, symbol];
        }

        private Dictionary<int, Dictionary<int, double>> Forward(int[] signal, out double[] scales)
        {
            Dictionary<int, Dictionary<int, double>> forwardVariables = new Dictionary<int, Dictionary<int, double>>();
            scales = new double[signal.Length];

            if (signal.Length == 0)
            {
                return forwardVariables;
            }

            double probability = 0;

            Dictionary<int, double> stateEmits = new Dictionary<int, double>();

            foreach (int i in emittents[signal[0]])
            {
                probability = (initialDistribution[i] * emissionProbabilities[i, signal[0]]);

                stateEmits.Add(i, probability);
                scales[0] += probability;
            }

            foreach (int i in emittents[signal[0]])
            {
                stateEmits[i] /= scales[0];
            }

            forwardVariables.Add(0, stateEmits);

            //foreach (int i in emittents[signal[0]])
            //{
            //    forwardVariables[0, i] = initialDistribution[i] * emissionProbabilities[i, signal[0]];
            //}

            for (int t = 1; t < signal.Length; t++)
            {
                stateEmits = new Dictionary<int, double>();

                foreach (int j in emittents[signal[t]])
                {
                    double score = 0.0;

                    foreach (int i in transitionsIn[j])
                    {
                        double forwardVariable;

                        if (forwardVariables[(t - 1)].TryGetValue(i, out forwardVariable))
                        {
                            score += (forwardVariable * transitionProbabilities[i, j]);
                        }
                    }

                    score *= emissionProbabilities[j, signal[t]];

                    stateEmits.Add(j, score);
                    scales[t] += score;
                }

                foreach (int j in emittents[signal[t]])
                {
                    stateEmits[j] /= scales[t];
                }

                forwardVariables.Add(t, stateEmits);
            }

            return forwardVariables;

            //for (int t = 1; t < signal.Length; t++)
            //{
            //    foreach (int j in emittents[signal[t]])
            //    {
            //        double score = 0.0;

            //        foreach (int i in transitionsIn[j])
            //        {
            //            score += (forwardVariables[(t - 1), i] * transitionProbabilities[i, j]);
            //        }

            //        forwardVariables[t, j] = (score * emissionProbabilities[j, signal[t]]);
            //    }
            //}

            //return forwardVariables;
        }

        private Dictionary<int, Dictionary<int, double>> Backward(int[] signal, double[] scales)
        {
            Dictionary<int, Dictionary<int, double>> backwardVariables = new Dictionary<int, Dictionary<int, double>>();

            if (signal.Length == 0)
            {
                return backwardVariables;
            }

            Dictionary<int, double> stateEmits = new Dictionary<int,double>();

            foreach(int i in emittents[signal[(signal.Length - 1)]])
            {
                stateEmits.Add(i, (1 / scales[(signal.Length - 1)]));

                //backwardVariables[(signal.Length - 1), i] = 1;
            }

            backwardVariables.Add((signal.Length - 1), stateEmits);

            for (int t = (signal.Length - 2); t >= 0; t--)
            {
                stateEmits = new Dictionary<int, double>();

                foreach (int i in emittents[signal[t]])
                {
                    double score = 0.0;

                    foreach (int j in transitionsOut[i])
                    {
                        double backwardVariable;

                        if (backwardVariables[(t + 1)].TryGetValue(j, out backwardVariable))
                        {
                            score += (transitionProbabilities[i, j] * emissionProbabilities[j, signal[(t + 1)]] * backwardVariable);
                        }
                    }

                    stateEmits.Add(i, (score / scales[t]));
                }

                backwardVariables.Add(t, stateEmits);
            }

            return backwardVariables;

            //for (int t = (signal.Length - 2); t >= 0; t--)
            //{
            //    foreach (int i in emittents[signal[t]])
            //    {
            //        double score = 0.0;

            //        foreach (int j in transitionsOut[i])
            //        {
            //            score += (transitionProbabilities[i, j] * emissionProbabilities[j, signal[(t + 1)]] * backwardVariables[(t + 1), j]);
            //        }

            //        backwardVariables[t, i] = score;
            //    }
            //}

            //return backwardVariables;
        }

        public double Evaluate(int[] signal)
        {
            if (signal.Length == 0)
            {
                return 1.0;
            }

            double[] scales;

            Dictionary<int, Dictionary<int, double>> forwardVariables = Forward(signal, out scales);

            return (forwardVariables[(signal.Length - 1)].Values.Sum() * scales[(signal.Length - 1)]);
        }

        public void Learn(SequenceData trainingData, double threshold)
        {
            int[][] signals = trainingData.GetNonempty();

            Dictionary<int, Dictionary<int, Dictionary<int, double>>>[] epsilon = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>[signals.Length];
            Dictionary<int, Dictionary<int, double>>[] gamma = new Dictionary<int, Dictionary<int, double>>[signals.Length];

            double oldLikelihood = Double.MinValue;
            double likelihood = 0.0;

            double scale = 0.0;
            double score = 0.0;

            bool stop = false;

            while (!stop)
            {
                for (int i = 0; i < signals.Length; i++)
                {
                    epsilon[i] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                    gamma[i] = new Dictionary<int, Dictionary<int, double>>();

                    double[] scaling;

                    Dictionary<int, Dictionary<int, double>> forwardVariables = Forward(signals[i], out scaling);
                    Dictionary<int, Dictionary<int, double>> backwardVariables = Backward(signals[i], scaling);

                    for (int t = 0; t < signals[i].Length; t++)
                    {
                        Dictionary<int, double> symbolGamma = new Dictionary<int, double>();
                        
                        scale = 0.0;

                        foreach (int j in forwardVariables[t].Keys)
                        {
                            if (backwardVariables[t].TryGetValue(j, out score))
                            {
                                score *= forwardVariables[t][j];

                                symbolGamma.Add(j, score);
                                scale += score;
                            }
                        }

                        if (scale != 0)
                        {
                            for (int j = 0; j < symbolGamma.Keys.Count; j++)
                            {
                                symbolGamma[symbolGamma.Keys.ElementAt(j)] /= scale;
                            }
                        }

                        gamma[i].Add(t, symbolGamma);
                    }

                    for (int t = 0; t < (signals[i].Length - 1); t++)
                    {
                        Dictionary<int, Dictionary<int, double>> symbolEpsilon = new Dictionary<int, Dictionary<int, double>>();

                        scale = 0.0;

                        foreach (int j in forwardVariables[t].Keys)
                        {
                            int[] validStates = backwardVariables[(t + 1)].Keys.Intersect(transitionsOut[j]).Intersect(emittents[signals[i][(t + 1)]]).ToArray();

                            Dictionary<int, double> transitionEpsilon = new Dictionary<int, double>();

                            foreach (int k in validStates)
                            {
                                score = (forwardVariables[t][j] * backwardVariables[(t + 1)][k] * emissionProbabilities[k, signals[i][(t + 1)]] * transitionProbabilities[j, k]);
                                scale += score;

                                transitionEpsilon.Add(k, score);
                            }

                            symbolEpsilon.Add(j, transitionEpsilon);
                        }

                        if (scale != 0)
                        {
                            for (int j = 0; j < symbolEpsilon.Keys.Count; j++)
                            {
                                for (int k = 0; k < symbolEpsilon[symbolEpsilon.Keys.ElementAt(j)].Keys.Count; k++)
                                {
                                    symbolEpsilon[symbolEpsilon.Keys.ElementAt(j)][symbolEpsilon[symbolEpsilon.Keys.ElementAt(j)].Keys.ElementAt(k)] /= scale;
                                }
                            }
                        }

                        epsilon[i].Add(t, symbolEpsilon);
                    }

                    likelihood += Math.Log(forwardVariables[(signals[i].Length - 1)].Values.Sum() * scaling[(signals[i].Length - 1)]);
                }

                likelihood /= signals.Length;

                if (Math.Abs(likelihood - oldLikelihood) <= threshold)
                {
                    stop = true;
                }
                else
                {
                    oldLikelihood = likelihood;
                    likelihood = 0.0;

                    for (int i = 0; i < NumberOfStates; i++)
                    {
                        double sum = 0.0;

                        for (int j = 0; j < signals.Length; j++)
                        {
                            if (gamma[j][0].TryGetValue(i, out score))
                            {
                                sum += score;
                            }
                        }

                        initialDistribution[i] = (sum / signals.Length);
                    }

                    double[] gammaSum = new double[NumberOfStates];

                    for (int i = 0; i < NumberOfStates; i++)
                    {
                        for (int k = 0; k < signals.Length; k++)
                        {
                            for (int l = 0; l < signals[k].Length; l++)
                            {
                                if (gamma[k][l].TryGetValue(i, out score))
                                {
                                    gammaSum[i] += score;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < NumberOfStates; i++)
                    {
                        foreach (int j in transitionsIn[i])
                        {
                            double epsilonSum = 0.0;

                            for (int k = 0; k < signals.Length; k++)
                            {
                                for (int l = 0; l < (signals[k].Length - 1); l++)
                                {
                                    Dictionary<int, double> epsilonValues;

                                    if (epsilon[k][l].TryGetValue(i, out epsilonValues) && epsilonValues.TryGetValue(j, out score))
                                    {
                                        epsilonSum += score;
                                    }
                                }
                            }

                            transitionProbabilities[i, j] = (epsilonSum / gammaSum[i]);
                        }
                    }

                    for (int i = 0; i < NumberOfStates; i++)
                    {
                        foreach (int j in emissions[i])
                        {
                            double symbolSum = 0.0;

                            for (int k = 0; k < signals.Length; k++)
                            {
                                for (int l = 0; l < signals[k].Length; l++)
                                {
                                    if (signals[k][l] == j)
                                    {
                                        if (gamma[k][l].TryGetValue(i, out score))
                                        {
                                            symbolSum += score;
                                        }
                                    }
                                }
                            }

                            emissionProbabilities[i, j] = (symbolSum / gammaSum[i]);
                        }
                    }
                }
            }
        }

        public int[] Viterby(int[] signal, out double probability)
        {
            double[,] scores = new double[signal.Length, NumberOfStates];
            int[,] parents = new int[signal.Length, NumberOfStates];

            int[] optimalSequence = new int[signal.Length];
            probability = 1.0;

            if (signal.Length == 0)
            {
                return optimalSequence.ToArray();
            }

            int minState;
            double minScore;
            double score;

            //Initialisation
            for (int i = 0; i < NumberOfStates; i++)
            {
                scores[0, i] = (-Math.Log(initialDistribution[i]) - Math.Log(EmissionProbability(i, signal[0])));
                parents[0, i] = (-1);
            }

            //Iteration
            for (int t = 1; t < signal.Length; t++)
            {
                foreach (int j in emittents[signal[t]])
                {
                    minState = (-1);
                    minScore = Double.MaxValue;

                    foreach(int i in transitionsIn[j])
                    {
                        score = (scores[(t - 1), i] - Math.Log(transitionProbabilities[i, j]));

                        if (score < minScore)
                        {
                            minState = i;
                            minScore = score;
                        }
                    }

                    scores[t, j] = (minScore - Math.Log(emissionProbabilities[j, signal[t]]));
                    parents[t, j] = minState;
                }
            }

            //Termination
            minState = (-1);
            minScore = Double.MaxValue;

            for (int i = 0; i < NumberOfStates; i++)
            {
                if (minScore > scores[(signal.Length - 1), i])
                {
                    minScore = scores[(signal.Length - 1), i];
                    minState = i;
                }
            }

            probability = minScore;
            optimalSequence[(signal.Length - 1)] = minState;

            //Backtracking
            for (int t = (signal.Length - 2); t >= 0; t--)
            {
                optimalSequence[t] = parents[(t + 1), optimalSequence[(t + 1)]];
            }

            return optimalSequence.ToArray();
        }
    }
}