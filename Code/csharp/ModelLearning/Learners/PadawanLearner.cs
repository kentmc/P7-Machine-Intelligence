using System;
using System.Linq;
using System.IO;

namespace ModelLearning.Learners {
 

	class PadawanLearner : Learner {

		public override double CalculateProbability(int[] sequence) {

				
			throw new NotImplementedException();
		}	

       		public override void Learn(SequenceData trainingData, 
				SequenceData validationData, SequenceData testData) {



				throw new NotImplementedException();
	
	
		}

		public override string Name() {
			return "Padawan Learner";
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

