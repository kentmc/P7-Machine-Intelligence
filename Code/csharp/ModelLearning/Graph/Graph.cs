using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelLearning {
    public class HMMGraph {

        public List<Node> Nodes;
        public int NumSymbols;

        public HMMGraph(int numSymbols) {
            NumSymbols = numSymbols;
            Nodes = new List<Node>();
        }

        public void AddNode(Node n) {
            Nodes.Add(n);
        }

        public int NumNodes {
            get { return Nodes.Count(); }
        }

        public Node GetNode(int index) {
            return Nodes[index];
        }

        private void NormalizeInitial() {
            double sum = Nodes.Sum(n => n.InitialProbability);
            foreach (Node n in Nodes) {
                if (sum == 0)
                    n.InitialProbability = 1.0 / Nodes.Count;
                n.InitialProbability /= sum;
            }
        }
        private void NormalizeEmissions() {
            foreach (Node n in Nodes){
                double sum = n.Emissions.Sum(e => e.Value);
                if (sum == 0)
                    for (int i = 0; i < NumSymbols; i++)
                        n.SetEmission(i, 1.0 / NumSymbols);
                else
                    for (int i = 0; i < NumSymbols; i++)
                        if (n.Emissions.ContainsKey(i))
                            n.SetEmission(i, n.Emissions[i] / sum);
                        else
                            n.SetEmission(i, 0);
            }
        }
        private void NormalizeTransitions() {
            foreach (Node from in Nodes) {
                double sum = from.Transitions.Sum(t => t.Value);
                if (sum == 0)
                    foreach (Node to in Nodes)
                        from.SetTransition(to, 1.0 / Nodes.Count);
                else
                    foreach (Node to in Nodes)
                        if (from.Transitions.ContainsKey(to))
                            from.SetTransition(to, from.Transitions[to] / sum);
                        else
                            from.SetTransition(to, 0);
            }
        }

        public void Normalize() {
            NormalizeInitial();
            NormalizeEmissions();
            NormalizeTransitions();
        }
    }
}
