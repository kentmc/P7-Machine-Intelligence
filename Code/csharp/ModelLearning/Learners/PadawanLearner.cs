using System;
using System.Linq;
using System.IO;
using Accord.Statistics.Models.Markov;

namespace ModelLearning.Learners {
 
	class PadawanLearner : Learner {
    	private const double EMPTY_SEQUENCE_PROBABILITY = 1.0;
    	private const double TRANSITION_UNIFORMITY_THRESHOLD = 0.0;
    	private const double EMISSION_UNIFORMITY_THRESHOLD = 0.0;
		private const double TOLERANCE = 2.0;
		private const int ITERATIONS = 20;
    	private HiddenMarkovModel hmm;

    	public override string Name() {
    		return "Padawan Learner";
    	}

    	public override double CalculateProbability(int[] sequence,
				                					bool logarithm = false) {
    		if (sequence.Length == 0) {
    			return EMPTY_SEQUENCE_PROBABILITY;
    		} else {
   				return hmm.Evaluate(sequence);	
    		}
    	}	
       		
		public override void Learn(SequenceData trainingData, 
				SequenceData validationData, SequenceData testData) {


			int[] O; // observed sequences.

			// 1. convert to hmm to graph model.

			HMMGraph hmmGraph = ModelConverter.HMM2Graph(hmm);				

			// 2. find argmax gamma

			
			

	   		// 3. split node if transition or emission probs are above uniformity threshold. 


	   		// 4. assign new probs and normalize.



	  		// 5. convert graph model back to hmm

			hmm = ModelConverter.Graph2HMM(hmmGraph);

	   		// 6. relearn model using BW.

			hmm.Learn(trainingData.GetAll(),ITERATIONS,TOLERANCE);


          	throw new NotImplementedException();
      	}

		private double ComputeGamma(Node n, HMMGraph G, int t, int[] O) {

			return (ComputeForward(n,G,t,O) * ComputeBackward(n, G, t, O)) / ComputeLikelihood(G,O); 
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

					sum += n.Transitions[ni] * ni.Emissions[O[t+1]] * ComputeBackward(ni, G, t+1, O);
				}

				return sum;
			}	
		}




      	private void RunBW() {

			throw new NotImplementedException();
      	}

      	public override void Initialise(LearnerParameters parameters, 
				int iteration) {


        	throw new NotImplementedException();
      	}

      	public override void Save(StreamWriter outputWriter, StreamWriter csvWriter) {

			throw new NotImplementedException();
      	}
  	}
}

