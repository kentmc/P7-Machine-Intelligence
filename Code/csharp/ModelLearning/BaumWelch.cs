using System;
using System.Linq;
using System.IO;

namespace ModelLearning {

	public class BaumWelch {
		HMMGraph graph;

		public HMMGraph Learn(HMMGraph hmm, int[] [] Observations) {

			Initialize(hmm);

			foreach(int[] O in Observations) {

				ReInitialize();

				reestimateInitialProbs(O);
				reestimateTransitions(O);
				reestimateEmissions(O);
			}

			return graph;
		}

		private void ReInitialize() {

			// reset dynProg 
		}

		private void reestimateInitialProbs(int[] O) {


			foreach(Node n in graph.Nodes) {
				
				n.InitialProbability = ComputeGamma(n, graph, 0, O); 	
			}
		}

		private void reestimateTransitions(int[] O) {

			foreach(Node na in graph.Nodes) {

				foreach(Node nb in graph.Nodes) {

					double ksi = 0;
					double gamma = 0;
					for(int t=0; t<O.Length; t++) {
				
						ksi += ComputeKsi(na, nb, graph, t,O);
						gamma += ComputeGamma(na, graph, t, O);
					}
					na.SetTransition(nb, ksi / gamma);
				}
			}
		}

		private void reestimateEmissions(int[] O) {

			foreach(Node n in graph.Nodes) {

				Dictionary<int,double> gammaobs;
				double gamma = 0;

				for(int t=0;t<O.Length;t++) {

					double current = ComputeGamma(n,graph,t,O);
					gammaobs[O[t]] += current; 
					gamma += current; 
				}	
				
				foreach(int i in gammaobs.Keys) {

					n.SetEmissions[i] = gammaobs[i] / gamma;
				}
		}

		private double ComputeKsi(Node na, Node nb, HMMGraph G, 
				int t, int[] O) {

			return ComputeForward(na, G, t, O) * na.Transitions[nb] 
				* nb.Emissions[O[t+1]] * ComputeBackward(nb,G,t+1,O);	
		}

		private double ComputeGamma(Node n, HMMGraph G, int[] O) {

			double sum = 0;
			for(int t=0; t<O.Length; t++) {
			
				sum += ComputeGamma(n, G, t, O);
			}
			return sum;
		}

		private double ComputeGamma(Node n, HMMGraph G, int t, int[] O) {

			return (ComputeForward(n,G,t,O) * ComputeBackward(n, G, t, O)) 
				/ ComputeLikelihood(G,O); 
		}

		private double ComputeLikelihood(HMMGraph G, int[] O) {

			double likelihood = 0;

			foreach(Node x in G.Nodes) {
				likelihood += x.InitialProbability * ComputeBackward(x, G, 0, O);
			}
		
			return likelihood;
		}

		private double ComputeForward(Node n, HMMGraph G, int t, int[] O) {

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

		private double ComputeBackward(Node n, HMMGraph G, int t, int[] O) {

			if(t==O.Length) {

				return 1.0;

			} else {

				double sum = 0;
				foreach(Node ni in G.Nodes) {

					sum += n.Transitions[ni] * ni.Emissions[O[t+1]] 
						* ComputeBackward(ni, G, t+1, O);
				}

				return sum;
			}	
		}
	}
}
