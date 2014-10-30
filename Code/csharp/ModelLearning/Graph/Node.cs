using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLearning {
    class Node {
        public double InitialProbability;
        public Dictionary<Node, double> Transitions;
        public Dictionary<int, double> Emissions; 

        public Node() {
            InitialProbability = 0;
            Transitions = new Dictionary<Node, double>();
            Emissions = new Dictionary<int, double>();
        }


        /// <summary>
        /// Sets a transition. If a transition exists for the same node, it is overwritten.
        /// If probability is 0, the transition is removed
        /// </summary>
        /// <param name="node"></param>
        /// <param name="prob"></param>
        internal void SetTransition(Node node, double prob) {
            if (prob == 0) {
                if (Transitions.ContainsKey(node))
                    Transitions.Remove(node);
            }
            else {
                if (Transitions.ContainsKey(node))
                    Transitions[node] = prob;
                else
                    Transitions.Add(node, prob);
            }
        }


        /// <summary>
        /// Sets an emission probability. If an emission exists for the same symbol, the probability is overwritten.
        /// If the probability is set to 0, it is removed.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="prob"></param>
        internal void SetEmission(int symbol, double prob) {
            if (prob == 0) {
                if (Emissions.ContainsKey(symbol))
                    Emissions.Remove(symbol);
            }
            else {
                if (Emissions.ContainsKey(symbol))
                    Emissions[symbol] = prob;
                else
                    Emissions.Add(symbol, prob);
            }
        }
    }
}
