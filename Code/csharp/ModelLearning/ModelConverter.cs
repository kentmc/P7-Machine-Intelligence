using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;

namespace ModelLearning {
    public static class ModelConverter {
        public static HMMGraph HMM2Graph(HiddenMarkovModel hmm){
            HMMGraph g = new HMMGraph(hmm.Symbols);
            Node[] nodes = new Node[hmm.States];
            for (int i = 0; i < hmm.States; i++) {
                nodes[i] = new Node();
                g.AddNode(nodes[i]);
            }
            for (int i = 0; i < hmm.States; i++) {
                nodes[i].InitialProbability = hmm.Probabilities[i];
                for (int j = 0; j < hmm.States; j++)
                    nodes[i].SetTransition(nodes[j], hmm.Transitions[i, j]);
                for (int k = 0; k < hmm.Symbols; k++)
                    nodes[i].SetEmission(k, hmm.Emissions[i, k]);
            }
            return g;
        }



        /// <summary>
        /// Constructs a HMM from a HMMGraph.
        /// Remember to call the Normalize method on the HMMGraph if necessary
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static HiddenMarkovModel Graph2HMM(HMMGraph g) {
            double[] initial = new double[g.Nodes.Count];
            for (int i = 0; i < g.NumNodes; i++)
                initial[i] = g.Nodes[i].InitialProbability;

            double[,] transitions = new double[g.NumNodes, g.NumNodes];
            double[,] emissions = new double[g.NumNodes, g.NumSymbols];

            for (int i = 0; i < g.NumNodes; i++) {
                foreach (KeyValuePair<Node, double> toProb in g.Nodes[i].Transitions.ToList()) {
                    int toNodeIndex = g.Nodes.IndexOf(toProb.Key);
                    transitions[i, toNodeIndex] = toProb.Value;
                }
                foreach (KeyValuePair<int, double> symbolProb in g.Nodes[i].Emissions)
                    emissions[i, symbolProb.Key] = symbolProb.Value;
            }
            return new HiddenMarkovModel(transitions, emissions, initial);
        }


        private static bool[] FindReachableNodes(HiddenMarkovModel hmm) {
            bool[] reachable = hmm.Probabilities.Select(p => (p != 0)).ToArray();

            List<int> newlyReached = new List<int>();
            for (int i = 0; i < hmm.States; i++)
                if (reachable[i])
                    newlyReached.Add(i);

            while (newlyReached.Count > 0) {
                List<int> nextNewlyReached = new List<int>();
                foreach (int i in newlyReached) {
                    for (int j = 0; j < hmm.States; j++)
                        if (hmm.Transitions[i, j] != 0 && !reachable[i]) {
                            nextNewlyReached.Add(j);
                            reachable[j] = true;
                        }
                }
                newlyReached = nextNewlyReached;
            }
            return reachable;
        }
    }
}
