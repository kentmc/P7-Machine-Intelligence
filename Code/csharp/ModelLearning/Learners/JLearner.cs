using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners
{
    class JLearner : Learner
    {
        private Random random = new Random();
        private SparseHiddenMarkovModel hmm;

        private double threshold;

        public JLearner(double threshold)
        {
            this.threshold = threshold;
        }

        public override double CalculateProbability(int[] sequence)
        {
            if (sequence.Length == 0)
            {
                return 1.0;
            }
            else
            {
                return hmm.Evaluate(sequence);
            }
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            hmm = SparseHiddenMarkovModel.FromCompleteGraph(1, trainingData.NumSymbols);

            double temperature = 2;
            double epsilon = 1.0;

            double likelihood = 0.0;
            double newLikelihood = Double.MinValue;

            double lastSparsity = hmm.TransitionSparsity;

            int stagnation = 1;

            do
            {
                if (temperature > 2)
                {
                    HMMGraph graph = hmm.ToGraph();

                    CutEdges(graph, epsilon);
                    if (hmm.TransitionSparsity != lastSparsity)
                    {
                        lastSparsity = hmm.TransitionSparsity;
                        stagnation = Math.Max(1, (stagnation - 1));
                    }
                    else
                    {
                        stagnation++;
                    }

                    //int numberOfStatesToAdd = Math.Max(0, (int)Math.Min(hmm.NumberOfStates, Math.Ceiling(Math.Log(Math.Pow(Math.Log(newLikelihood - likelihood), (1 / stagnation)) / (Math.Sqrt(temperature) * threshold)))));
                    int numberOfStatesToAdd = (((stagnation / temperature) > threshold) ? 1 : 0);
                    foreach (int weakPoint in IdentifyWeakStates(validationData, numberOfStatesToAdd))
                    {
                        SplitState(graph, weakPoint);
                        stagnation = 1;
                    }

                    if (numberOfStatesToAdd == 0)
                    {
                        stagnation *= 2;
                    }

                    hmm = SparseHiddenMarkovModel.FromGraph(graph);

                    WriteLine(String.Format("Added {0} states", numberOfStatesToAdd));
                }

                //temperature *= Math.Max(2, Math.Sqrt(hmm.NumberOfStates));
                temperature *= Math.Max(2, stagnation);
                epsilon = (1 / Math.Log(temperature));

                //double bwThreshold = Math.Pow(Math.Max(threshold, (1 / (-Math.Min((-1), Math.Log(Math.Min((1 - threshold), (1 / temperature)) / (1 - threshold)))))), stagnation);
                int bwIterations = Math.Max(1, (int)Math.Log(stagnation * temperature * threshold));

                //WriteLine(String.Format("Running Baum-Welch with threshold {0}...", bwThreshold));
                WriteLine(String.Format("Running Baum-Welch with {0} iterations...", bwIterations));

                //hmm.Learn(trainingData.GetNonempty(), bwThreshold);
                hmm.Learn(trainingData.GetNonempty(), 0.0, bwIterations);

                likelihood = newLikelihood;
                newLikelihood = 0.0;

                foreach (int[] signal in trainingData.GetNonempty())
                {
                    newLikelihood += hmm.Evaluate(signal, true);
                }

                WriteLine(String.Empty);
                WriteLine(String.Format("Stagnation: {0}", stagnation));
                WriteLine(String.Format("Epsilon: {0}", epsilon));
                WriteLine(String.Format("Number of HMM States: {0}", hmm.NumberOfStates));
                WriteLine(String.Format("Transition Sparsity; {0}", hmm.TransitionSparsity));
                WriteLine(String.Format("Log Likelihood: {0}", newLikelihood));
                WriteLine(String.Empty);
            }
            while ((Math.Abs(newLikelihood - likelihood) / Math.Sqrt(temperature)) > threshold);
        }

        private IEnumerable<int> IdentifyWeakStates(SequenceData validationData, int numberOfStates = 1) //Using Viterby
        {
            double[] scores = new double[hmm.NumberOfStates];

            foreach(int[] signal in validationData.GetNonempty())
            {
                double probability;
                double[] subscore = new double[hmm.NumberOfStates];
                int[] occurenceCount = new int[hmm.NumberOfStates];

                int[] hiddenStateSequence = hmm.Viterby(signal, out probability);

                if (hiddenStateSequence == null)
                {
                    continue;
                }

                for (int t = 0; t < signal.Length; t++)
                {
                    subscore[hiddenStateSequence[t]] += hmm.EmissionProbability(hiddenStateSequence[t], signal[t]);
                    occurenceCount[hiddenStateSequence[t]]++;
                }

                for (int i = 0; i < hmm.NumberOfStates; i++)
                {
                    if (occurenceCount[i] > 0)
                    {
                        subscore[i] = ((probability * subscore[i]) / occurenceCount[i]);
                    }

                    scores[i] += subscore[i];
                }
            }

            return Enumerable.Range(0, hmm.NumberOfStates).OrderBy(i => scores[i]).Take(numberOfStates);

            //double worstScore = Double.MaxValue;
            //int worstPerformer = 0;
            //for (int i = 0; i < hmm.NumberOfStates; i++)
            //{
            //    if (scores[i] < worstScore)
            //    {
            //        worstScore = scores[i];
            //        worstPerformer = i;
            //    }
            //}

            //yield return worstPerformer;
        }

        private void SplitState(HMMGraph graph, int state)
        {
            Node newNode = new Node();

            foreach (Node node in graph.Nodes)
            {
                if (node.Transitions.ContainsKey(graph.Nodes[state]))
                {
                    //node.SetTransition(newNode, node.Transitions[graph.Nodes[state]]);
                    node.SetTransition(newNode, random.NextDouble());
                }
            }

            foreach (Node node in graph.Nodes[state].Transitions.Keys)
            {
                //newNode.SetTransition(node, graph.Nodes[state].Transitions[node]);
                newNode.SetTransition(node, random.NextDouble());
            }

            foreach (int symbol in graph.Nodes[state].Emissions.Keys)
            {
                //newNode.SetEmission(symbol, graph.Nodes[state].Emissions[symbol]);
                newNode.SetEmission(symbol, random.NextDouble());
            }

            //newNode.InitialProbability = graph.Nodes[state].InitialProbability;
            newNode.InitialProbability = random.NextDouble();

            graph.Nodes.Add(newNode);

            graph.Normalize();
        }

        private void CutEdges(HMMGraph graph, double temperature)
        {
            foreach (Node node in graph.Nodes)
            {
                node.Transitions = node.Transitions.Keys.Where(n => (node.Transitions[n] >= temperature)).ToDictionary(n => n, n => node.Transitions[n]);
            }

            graph.Normalize();
        }

        public override string Name()
        {
            return "JLearner";
        }
    }
}
