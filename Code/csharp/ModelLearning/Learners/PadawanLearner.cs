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

      	private void runBW() {

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

