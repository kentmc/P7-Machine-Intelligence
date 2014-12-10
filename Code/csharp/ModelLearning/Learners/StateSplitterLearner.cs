using Accord.Statistics.Models.Markov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning.Learners
{
    class StateSplitterLearner : Learner
    {
        private Random random;
        //private SparseHiddenMarkovModel hmm;
        private HiddenMarkovModel hmm;

        private double threshold;
        private double epsilon;
        private double convergenceThreshold;

        private int NumberOfStates
        {
            get
            {
                //return hmm.NumberOfStates;
                return hmm.States;
            }
        }

        public StateSplitterLearner()
        {
            random = new Random();
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData)
        {
            //hmm = SparseHiddenMarkovModel.FromCompleteGraph(2, trainingData.NumSymbols);
            hmm = new HiddenMarkovModel(trainingData.NumSymbols, 4);

            double likelihood = 0.0;
            double newLikelihood = Double.MinValue;

            do
            {
                //HMMGraph graph = hmm.ToGraph();
                HMMGraph graph = ModelConverter.HMM2Graph(hmm);

                //CutEdges(graph, epsilon);

                double[] viterbyScores = ComputeViterbyScores(validationData, true);

                int[] statesToSplit = IdentifyWeakStates(viterbyScores).ToArray();
                foreach (int weakPoint in statesToSplit)
                {
                    SplitState(graph, weakPoint);
                }

                WriteLine(String.Format("Added {0} states", statesToSplit.Length));
                //WriteLine(String.Format("Removed {0} states", RemoveUnpopularStates(graph, viterbyScores)));

                //hmm = SparseHiddenMarkovModel.FromGraph(graph);
                hmm = ModelConverter.Graph2HMM(graph);

                WriteLine("Running Baum Welch...");
                //hmm.Learn(trainingData.GetNonempty(), 0.0, 8);
                hmm.Learn(trainingData.GetNonempty(), 8);

                likelihood = newLikelihood;
                newLikelihood = 0.0;

                foreach (int[] signal in validationData.GetNonempty())
                {
                    newLikelihood += hmm.Evaluate(signal, true);
                }

                WriteLine(String.Empty);
                WriteLine(String.Format("Number of HMM States: {0}", NumberOfStates));
                //WriteLine(String.Format("Transition Sparsity; {0}", hmm.TransitionSparsity));
                WriteLine(String.Format("Log Likelihood: {0}", newLikelihood));
                WriteLine(String.Empty);
            }
            while (Math.Abs(newLikelihood - likelihood) > convergenceThreshold);
        }

        private double[] ComputeViterbyScores(SequenceData validationData, bool normalise = false)
        {
            double[] scores = new double[NumberOfStates];

            foreach (int[] signal in validationData.GetNonempty())
            {
                double probability;
                double[] subscore = new double[NumberOfStates];
                int[] occurenceCount = new int[NumberOfStates];

                //int[] hiddenStateSequence = hmm.Viterby(signal, out probability);
                int[] hiddenStateSequence = hmm.Decode(signal, out probability);

                if (hiddenStateSequence == null)
                {
                    continue;
                }

                for (int t = 0; t < signal.Length; t++)
                {
                    //subscore[hiddenStateSequence[t]] += hmm.EmissionProbability(hiddenStateSequence[t], signal[t]);
                    subscore[hiddenStateSequence[t]] += hmm.Emissions[hiddenStateSequence[t], signal[t]];
                    occurenceCount[hiddenStateSequence[t]]++;
                }

                for (int i = 0; i < NumberOfStates; i++)
                {
                    if (occurenceCount[i] > 0)
                    {
                        subscore[i] = ((probability * subscore[i]) / occurenceCount[i]);
                    }

                    scores[i] += subscore[i];
                }
            }

            for (int i = 0; i < NumberOfStates; i++)
            {
                scores[i] /= validationData.Count;
            }

            if (normalise)
            {
                double sum = 0;

                for (int i = 0; i < NumberOfStates; i++)
                {
                    sum += scores[i];
                }

                for (int i = 0; i < NumberOfStates; i++)
                {
                    scores[i] /= sum;
                }
            }

            return scores;
        }

        private IEnumerable<int> IdentifyWeakStates(double[] scores)
        {
            for (int i = 0; i < NumberOfStates; i++)
            {
                if ((threshold / NumberOfStates) < scores[i])
                {
                    yield return i;
                }
            }
        }

        private IEnumerable<int> IdentifyWeakStates(SequenceData validationData)
        {
            double[] scores = ComputeViterbyScores(validationData, true);

            return IdentifyWeakStates(scores);
        }

        private IEnumerable<int> IdentifyWeakStates(SequenceData validationData, int numberOfStates = 1)
        {
            double[] scores = ComputeViterbyScores(validationData);

            return Enumerable.Range(0, NumberOfStates).OrderBy(i => scores[i]).Take(numberOfStates);
        }

        private void SplitState(HMMGraph graph, int state)
        {
            Node newNode = new Node();

            foreach (Node node in graph.Nodes)
            {
                if (node.Transitions.ContainsKey(graph.Nodes[state]))
                {
                    //node.SetTransition(newNode, (node.Transitions[graph.Nodes[state]] / 2));
                    //node.Transitions[graph.Nodes[state]] /= 2;
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

            //newNode.InitialProbability = (graph.Nodes[state].InitialProbability / 2);
            //graph.Nodes[state].InitialProbability /= 2;
            newNode.InitialProbability = random.NextDouble();

            graph.Nodes.Add(newNode);

            graph.Normalize();
        }

        private void CutEdges(HMMGraph graph, double epsilon)
        {
            foreach (Node node in graph.Nodes)
            {
                if (node.InitialProbability < epsilon)
                {
                    node.InitialProbability = 0;
                }

                node.Transitions = node.Transitions.Keys.Where(n => (node.Transitions[n] >= epsilon)).ToDictionary(n => n, n => node.Transitions[n]);
            }

            graph.Normalize();
        }

        private int RemoveUnpopularStates(HMMGraph graph, double[] scores)
        {
            int[] unpopularStates = Enumerable.Range(0, NumberOfStates).Where(i => (scores[i] == 0)).OrderByDescending(i => i).ToArray();

            foreach (Node node in graph.Nodes)
            {
                foreach (int unpopularState in unpopularStates)
                {
                    node.Transitions.Remove(graph.Nodes[unpopularState]);
                }
            }

            foreach (int unpopularState in unpopularStates)
            {
                graph.Nodes.RemoveAt(unpopularState);
            }

            return unpopularStates.Length;
        }

        private int RemoveDeadStates(HMMGraph graph)
        {
            int deadNodes = 0;
            Dictionary<Node, bool> hasInTrans = graph.Nodes.ToDictionary(node => node, _ => false);

            foreach (Node node in graph.Nodes)
            {
                if (node.InitialProbability > 0)
                {
                    hasInTrans[node] = true;
                }

                foreach (Node trans in node.Transitions.Keys)
                {
                    hasInTrans[trans] = true;
                }
            }

            for (int i = 0; i < graph.NumNodes; i++)
            {
                if (!hasInTrans[graph.Nodes[i]])
                {
                    deadNodes++;
                    graph.Nodes.RemoveAt(i);
                    i--;

                    continue;
                }

                if (graph.Nodes[i].Transitions.Count == 0)
                {
                    foreach (Node node in graph.Nodes)
                    {
                        node.Transitions.Remove(node);
                    }

                    deadNodes++;
                    graph.Nodes.RemoveAt(i);
                    i--;
                }
            }

            return deadNodes;
        }

        public override string Name()
        {
            return "Greedy State Splitter";
        }

        public override double CalculateProbability(int[] sequence, bool logarithm = false)
        {
            if (sequence.Length == 0)
            {
                return (logarithm ? 0.0 : 1.0);
            }
            else
            {
                return hmm.Evaluate(sequence, logarithm);
            }
        }

        public override void Initialise(LearnerParameters parameters, int iteration)
        {
            threshold = (parameters.Minimum + (iteration * parameters.StepSize));

            convergenceThreshold = (double)parameters.AdditionalParameters["threshold"];
            epsilon = (double)parameters.AdditionalParameters["epsilon"];
        }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter)
        {
            outputWriter.WriteLine("Threshold: {0}", threshold);
            outputWriter.WriteLine("Convergence Threshold: {0}", convergenceThreshold);
            outputWriter.WriteLine("Epsilon: {0}", epsilon);
            outputWriter.WriteLine();

            //hmm.Save(outputWriter, csvWriter);
        }
    }
}
