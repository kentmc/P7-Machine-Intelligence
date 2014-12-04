using System;
using System.Linq;
using System.IO;
using Accord.Statistics.Models.Markov;

namespace ModelLearning {

	public static class BaumWelch {



		public HMMGraph Learn(HMMGraph hmm) {

			
			// Initialize
			// compute gamma
			// compute ksi
			// reestimate model
		}

		private void Initialize() {

			// reset dynProg 
		}

		private double reestimateInitialProbs() {

		}

		private double reestimateTransitions() {

		}

		private double reestimateEmissions() {

		}

		public static double ComputeKsi(Node na, Node nb, HMMGraph G, int t, int[] O) {

			return ComputeForward(na, G, t, O) * na.Transitions[nb] * nb.Emissions[O[t+1]] * ComputeBackward(nb,G,t+1,O);	
		}

		public static double ComputeGamma(Node n, HMMGraph G, int[] O) {

			double sum = 0;
			for(int t=0; t<O.Length; t++) {
			
				sum += ComputeGamma(n, G, t, O);
			}
			return sum;
		}

		public static double ComputeGamma(Node n, HMMGraph G, int t, int[] O) {

			return (ComputeForward(n,G,t,O) * ComputeBackward(n, G, t, O)) / ComputeLikelihood(G,O); 
		}

		public static double ComputeLikelihood(HMMGraph G, int[] O) {

			double likelihood = 0;

			foreach(Node x in G.Nodes) {
				likelihood += x.InitialProbability * ComputeBackward(x, G, 0, O);
			}
		
			return likelihood;
		}

		public static double ComputeForward(Node n, HMMGraph G, int t, int[] O) {

			if(t==0) {

				return n.InitialProbability * n.Emissions[O[t]];
			} else {

				double sum = 0;

				foreach(Node ni in G.Nodes) {
					sum += ComputeForward(ni, G, t-1, O) * ni.Transitions[n];
				}

				return sum * n.Emissions[O[t]];
			}
		}

		public static double ComputeBackward(Node n, HMMGraph G, int t, int[] O) {

			if(t==O.Length) {

				return 1.0;

			} else {

				double sum = 0;
				foreach(Node ni in G.Nodes) {

					sum += n.Transitions[ni] * ni.Emissions[O[t+1]] * ComputeBackward(ni, G, t+1, O);
				}

				return sum;
			}	
		}
	}
}
