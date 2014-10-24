using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLearning {
    class Edge : IEquatable<Edge> {
        public int From, To;
        public double Weight;

        public Edge(int from, int to, double weight){
            this.From = from;
            this.To = to;
            this.Weight = weight;
        }

        public bool Equals(Edge other) {
            return From == other.From && To == other.To;
        }
    }
}
