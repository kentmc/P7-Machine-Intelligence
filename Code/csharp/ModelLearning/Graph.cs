using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelLearning {
    class HMMGraph {
        private List<int> nodes;

        //Dictionary[x] returns hash set of edges from node x to any other node.
        private Dictionary<int, Dictionary<int, double>> transitions;

        //Dictionary[x] returns a Dictionary y, where y[z] is the probability that state x emits the symbol z.
        private Dictionary<int, Dictionary<int, double>> emissions; 

        //Dictionary[x] returns the probability of starting in state x.
        private Dictionary<int, double> initial; 

        private int symbols;
        private int states;

        public HMMGraph(int symbols, int states) {
            this.states = states;
            this.symbols = symbols;
            transitions = new Dictionary<int, Dictionary<int, double>>();
            initial = new Dictionary<int, double>();
            emissions = new Dictionary<int, Dictionary<int, double>>();
            nodes = new List<int>();
        }

        public void RemoveTransition(int from, int to) {
            if (transitions.ContainsKey(from)) {
                if (transitions[from].ContainsKey(to))
                    transitions[from].Remove(to);
                if (transitions[from].Count == 0)
                    transitions.Remove(from);
            }
        }

        public void RemoveEmission(int state, int symbol) {
            if (emissions.ContainsKey(state)) {
                if (emissions[state].ContainsKey(symbol))
                    emissions[state].Remove(symbol);
                if (emissions[state].Count == 0)
                    emissions.Remove(state);
            }
        }

        public void RemoveInitial(int state) {
            if (initial.ContainsKey(state))
                initial.Remove(state);
        }

        public void SetTransition(int from, int to, double probability) {
            if (probability == 0) {
                //delete edge if it exists to keep sparsity
                RemoveTransition(from, to);
                return;
            }
              
            if (!transitions.ContainsKey(from)) {
                transitions.Add(from, new Dictionary<int, double>());
                transitions[from].Add(to, probability);
            }
            else if (transitions[from].ContainsKey(to))
                transitions[from][to] = probability;
            else
                transitions[from].Add(to, probability);
        }

        public int OutDegree(int node) {
            if (transitions.ContainsKey(node))
                return transitions[node].Count;
            else
                return 0;
        }

        internal void SetInitial(int state, double probability) {
            if (probability == 0) {
                RemoveInitial(state);
                return;
            }

            if (initial.ContainsKey(state))
                initial[state] = probability;
            else
                initial.Add(state, probability);
        }

        internal void SetEmission(int state, int symbol, double probability) {
            if (probability == 0) {
                RemoveEmission(state, symbol);
                return;
            }
            if (emissions.ContainsKey(state)) {
                if (emissions[state].ContainsKey(symbol))
                    emissions[state][symbol] = probability;
                else
                    emissions[state].Add(symbol, probability);
            }
            else {
                emissions.Add(state, new Dictionary<int, double>());
                emissions[state][symbol] = probability;
            }
        }
    }
}
