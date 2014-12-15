using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace ModelLearning {
    
	// TODO: Implement scaling.
	public class BaumWelch {
		private HMMGraph graph;
        private double[,] forward;
        private double[,] backward;
        private double[] c; // scaling coefficiets
        private const double UNASSIGNED = -1;
        private const double MINIMUM_PROB = 0.00001;

        //// default cTor.
        //public BaumWelch() : this(0, null) {

        //    throw new Exception("Need parameters");
        //}

        // Master cTor.
        public BaumWelch(int obslength, HMMGraph G)
        {
            graph = G;
            ReInitialize(obslength);
        }

		public HMMGraph Learn(HMMGraph hmm, int[] [] Observations) {

            // Setup algo
            graph = hmm;

			foreach(int[] O in Observations) {

				ReInitialize(O.Length);



				reestimateInitialProbs(O);
				reestimateTransitions(O);
				reestimateEmissions(O);

                graph.Normalize();
			}
			return graph;
		}

		private void ReInitialize(int obsLength) {

            // reset dynProg 
            forward = new double[obsLength, graph.Nodes.Count];
            backward = new double[obsLength, graph.Nodes.Count];
            c = new double[obsLength];

            for(int i=0;i<obsLength;i++) {

                for (int j = 0; j < graph.Nodes.Count; j++)
                {
                    forward[i,j] = UNASSIGNED;
                    backward[i,j] = UNASSIGNED;
                }
                c[i] = 0.0;
            }
		}

        private void reestimateInitialProbs(int[] O)
        {
            
			foreach(Node n in graph.Nodes) {
				
				n.InitialProbability = ComputeGamma(n, graph, 0, O); 	
			}
		}

		private void reestimateTransitions(int[] O) {

			foreach(Node na in graph.Nodes) {

				foreach(Node nb in graph.Nodes) {

					double ksi = 0;
					double gamma = 0;
					for(int t=0; t<O.Length-1; t++) {
				
						ksi += ComputeKsi(na, nb, graph, t,O);
						gamma += ComputeGamma(na, graph, t, O);
					}
					na.SetTransition(nb, ksi / gamma);
				}
			}
		}

		private void reestimateEmissions(int[] O) {

			foreach(Node n in graph.Nodes) {

				Dictionary<int,double> gammaobs = new Dictionary<int,double>();
				double gamma = 0;

				for(int t=0;t<O.Length;t++) {

					double current = ComputeGamma(n,graph,t,O);
                    if(gammaobs.ContainsKey(O[t])) {
					    gammaobs[O[t]] += current; 
                    }
                    else {
                        gammaobs.Add(O[t],current);
                    }
					gamma += current; 
				}

                n.Emissions.Clear(); // Reset emissions probs

				foreach(int k in gammaobs.Keys) {

					n.SetEmission(k, gammaobs[k] / gamma);
				}
			}
		}

		private double ComputeKsi(Node na, Node nb, HMMGraph G, 
				int t, int[] O) {

            return ComputeForward(na, G, t, O) 
                * (na.Transitions.Keys.Contains(nb) ? na.Transitions[nb] : MINIMUM_PROB)
                * (nb.Emissions.Keys.Contains(O[t+1]) ? nb.Emissions[O[t + 1]] : MINIMUM_PROB)
                * ComputeBackward(nb, G, t + 1, O);
		}

		public double ComputeGamma(Node n, HMMGraph G, int[] O) {

			double sum = 0;
			for(int t=0; t<O.Length; t++) {
			
				sum += ComputeGamma(n, G, t, O);
			}
			return sum;
		}

        // return a relative gamma value (Unscaled)
		private double ComputeGamma(Node n, HMMGraph G, int t, int[] O) {

            //double result = (ComputeForward(n,G,t,O) * ComputeBackward(n, G, t, O)) 
            //    / ComputeLikelihood(G,O);

            double fwd = ComputeForward(n, G, t, O);
            double bwd = ComputeBackward(n, G, t, O);
            double likelihood = ComputeLikelihood(G, O);

            double result = (fwd * bwd) / likelihood;
            
            return result;
		}

		private double ComputeLikelihood(HMMGraph G, int[] O) {

			double likelihood = 0;

			foreach(Node n in G.Nodes) {

                double bwd = ComputeBackward(n, G, 0, O);
                double init = n.InitialProbability;

                double result = init * bwd;

                return result;
				//likelihood += n.InitialProbability * ComputeBackward(n, G, 0, O);
			}
		
			return likelihood;
		}

        public void PreCompute(HMMGraph G, int[] O) {

            PreComputeForward(G,O);
            PreComputeBackward(G,O);
        }

        private void PreComputeForward(HMMGraph G, int[] O) {

            // Base step
            foreach (Node n in G.Nodes) {
                // calc coefficient
                c[0] += ComputeForward(n, G, 0, O);
            }

            // Scale
            foreach (Node n in G.Nodes) {
                forward[0, G.Nodes.IndexOf(n)] = forward[0, G.Nodes.IndexOf(n)] / c[0];
            }

            // Induction step
            for (int t = 1; t < O.Length; t++) {

                foreach (Node n in G.Nodes) {
                    c[t] += ComputeForward(n, G, t, O);
                }

                foreach (Node n in G.Nodes) {

                    forward[t, G.Nodes.IndexOf(n)] = forward[t, G.Nodes.IndexOf(n)] / c[t]; 
                }
            }
        }

        private void PreComputeBackward(HMMGraph G, int[] O) {

            for (int t = O.Length-1; t > -1; t--) {

                foreach (Node n in G.Nodes) {
                    // scaling
                    backward[t, G.Nodes.IndexOf(n)] = backward[t, G.Nodes.IndexOf(n)] / c[t]; 
                }
            }
        }

        private double ComputeForward(Node n, HMMGraph G, int t, int[] O) {

            if (forward[t,graph.Nodes.IndexOf(n)] == UNASSIGNED)
            {
                if (t == 0)
                {
                    forward[t,graph.Nodes.IndexOf(n)] = n.InitialProbability 
                        * (n.Emissions.Keys.Contains(O[t]) ? n.Emissions[O[t]] : MINIMUM_PROB);
                }
                else
                {
                    double sum = 0;

                    foreach (Node ni in G.Nodes)
                    {
                        sum += ComputeForward(ni, G, t - 1, O) 
                            * (ni.Transitions.Keys.Contains(n) ? ni.Transitions[n] : MINIMUM_PROB);
                    }
                    forward[t, graph.Nodes.IndexOf(n)] = sum 
                           * (n.Emissions.Keys.Contains(O[t]) ? n.Emissions[O[t]] : MINIMUM_PROB);
                }
            }
            return forward[t, graph.Nodes.IndexOf(n)];
		}

        private double ComputeBackward(Node n, HMMGraph G, int t, int[] O) {

            if (backward[t,graph.Nodes.IndexOf(n)] == UNASSIGNED)
            {
                if (t == O.Length - 1)
                {
                    backward[t, graph.Nodes.IndexOf(n)] = 1.0;
                }
                else
                {
                    double sum = 0;
                    foreach (Node ni in G.Nodes)
                    {
                        //sum += (n.Transitions.Keys.Contains(ni) ? n.Transitions[ni] : MINIMUM_PROB)
                        //    * (ni.Emissions.Keys.Contains(O[t+1]) ? ni.Emissions[O[t + 1]] : MINIMUM_PROB)
                        //    * ComputeBackward(ni, G, t + 1, O);
                        double trans = (n.Transitions.Keys.Contains(ni) ? n.Transitions[ni] : MINIMUM_PROB);
                        double emis = (ni.Emissions.Keys.Contains(O[t + 1]) ? ni.Emissions[O[t + 1]] : MINIMUM_PROB);
                        double bwd = ComputeBackward(ni, G, t + 1, O);

                        sum += trans * emis * bwd;

                        //if (trans == 0.0 || emis == 0.0 || bwd == 0.0) {
                        //    Console.WriteLine("stuff");
                        //}
                    }

                    backward[t, graph.Nodes.IndexOf(n)] = sum;
                }
            }
            return backward[t, graph.Nodes.IndexOf(n)];
		}
	}
}
