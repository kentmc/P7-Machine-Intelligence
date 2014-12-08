using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;

namespace ModelLearning.Learners {
    class SparseBaumWelchLearner : Learner {

		SparseHiddenMarkovModel hmm;
        double tolerance;
        int states;
        Random ran;

        /// <summary>
        /// This is an ordinary Baum Welch Learner, except that each node is connected to at most Log (node_count) other nodes
        /// </summary>
        /// <param name="states"></param>
        /// <param name="tolerance"></param>
        //public SparseBaumWelchLearner() {
		public SparseBaumWelchLearner() {
            ran = new Random();
        }

        public override double CalculateProbability(int[] sequence, bool logarithm = false) {
            if (sequence.Length == 0)
                return (logarithm ? 1.0 : 0.0);
            else
                return hmm.Evaluate(sequence, logarithm);
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData) {
            HMMGraph graph = new HMMGraph(trainingData.NumSymbols);

            //Add nodes and set initial and emission probabilities
            for (int i = 0; i < states; i++) {
                Node new_node = new Node();
                for (int s = 0; s < trainingData.NumSymbols; s++)
                    new_node.SetEmission(s, ran.NextDouble());
                new_node.InitialProbability = ran.NextDouble();
                graph.AddNode(new_node);
            }

            //Add random transmissions. Each node will have at most Log(n) edges in both directions
            List<Node> shuffled = graph.Nodes.Select(e => e).ToList();
            Utilities.Shuffle(shuffled);

            int numberOfTransitions = (int)Math.Ceiling(Math.Log(states));

            foreach (Node node in graph.Nodes)
            {
                for (int i = 0; i < numberOfTransitions; i++)
                {
                    Node target;
                    while (node.Transitions.ContainsKey(target = graph.Nodes[ran.Next(states)])) ;

                    node.SetTransition(target, ran.NextDouble());
                }
            }

            graph.Normalize();
			hmm = SparseHiddenMarkovModel.FromGraph(graph);
            hmm.Learn(trainingData.GetNonempty(), tolerance);
        }

        public override string Name() {
            return "Sparse Baum Welch Learner";
        }

        public override void Initialise(LearnerParameters parameters, int iteration)
        {
            tolerance = (double)parameters.AdditionalParameters["threshold"];
            states = (int)(parameters.Minimum + (iteration * parameters.StepSize));
        }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter)
        {
            outputWriter.WriteLine("States: {0}", hmm.NumberOfStates);
            outputWriter.WriteLine("Symbols: {0}", hmm.NumberOfSymbols);
            outputWriter.WriteLine("Threshold: {0}", tolerance);
            hmm.Save(outputWriter, csvWriter);
        }
    }
}
