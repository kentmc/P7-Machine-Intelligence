using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModelLearning {
    class HMMGraph {

        public HashSet<Node> Nodes;
        public int NumSymbols;
        public bool IsLogarithmic;

        public HMMGraph(int numSymbols, bool isLogarithmic) {
            NumSymbols = numSymbols;
            Nodes = new HashSet<Node>();
            IsLogarithmic = isLogarithmic;
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
