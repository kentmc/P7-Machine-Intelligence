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
            Emissions = new Dictionary<int,double>();
        }
    }
}
