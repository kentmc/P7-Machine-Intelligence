using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelLearning {
    class HMMGraph {

        public HashSet<Node> Nodes;
        public int NumSymbols;

        public HMMGraph(int numSymbols) {
            NumSymbols = numSymbols;
            Nodes = new HashSet<Node>();
        }

        public void AddNode(Node n) {
            Nodes.Add(n);
        }

        public void NormalizeInitialProbabilities() {
            double sum = Nodes.Sum(n => n.InitialProbability);
            foreach (Node n in Nodes) {
                if (sum == 0)
                    n.InitialProbability = 1.0 / Nodes.Count;
                n.InitialProbability /= sum;
            }
        }
    }
}
