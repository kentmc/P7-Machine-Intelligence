using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;

namespace ModelLearning {
    static class ModelConverter {
        public static HMMGraph HMM2Graph(HiddenMarkovModel hmm){
            HMMGraph g = new HMMGraph(hmm.Symbols, true);

            bool[] reachable = FindReachableNodes(hmm);
            Node[] nodes = new Node[hmm.States];

            for (int i = 0; i < hmm.States; i++) {
                if (reachable[i]) {
                    nodes[i] = new Node();
                    g.AddNode(nodes[i]);
                }
            }

                for (int i = 0; i < hmm.States; i++) {
                    if (reachable[i]) {
                        nodes[i].InitialProbability = hmm.Probabilities[i];
                        for (int j = 0; j < hmm.States; j++)
                            if (hmm.Transitions[i, j] != 0)
                                nodes[i].Transitions.Add(nodes[j], hmm.Transitions[i, j]);
                        for (int k = 0; k < hmm.Symbols; k++)
                            if (hmm.Emissions[i, k] != 0)
                                nodes[i].Emissions.Add(k, hmm.Emissions[i, k]);
                    }
                }
            return g;
        }

        public static HiddenMarkovModel Graph2HMM(HMMGraph g) {
            List<Node> nodes = g.Nodes.ToList();
            double[] initial = new double[g.Nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                initial[i] = nodes[i].InitialProbability;

            double[,] transitions = new double[nodes.Count, nodes.Count];
            double[,] emissions = new double[nodes.Count, g.NumSymbols];

            for (int i = 0; i < nodes.Count; i++) {
                foreach (KeyValuePair<Node, double> toProb in nodes[i].Transitions.ToList()) {
                    int toNodeIndex = nodes.IndexOf(toProb.Key);
                    transitions[i, toNodeIndex] = toProb.Value;
                }
                foreach (KeyValuePair<int, double> symbolProb in nodes[i].Emissions)
                    emissions[i, symbolProb.Key] = symbolProb.Value;
            }
            return new HiddenMarkovModel(transitions, emissions, initial, g.IsLogarithmic);
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
