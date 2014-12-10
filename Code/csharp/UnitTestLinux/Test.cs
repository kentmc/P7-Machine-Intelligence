using System;
using ModelLearning; 
using NUnit.Framework;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nunitlinux
{
	[TestFixture()]
	public class BaumWelchTest
	{
		private const int NUMBER_OF_SYMBOLS_IN_HMMGRAPH = 40;

		[Test()]
		public void LearnTest_validInput_ModelDescribingTheData ()
		{
			// Arrange
			HMMGraph hmm = new HMMGraph(NUMBER_OF_SYMBOLS_IN_HMMGRAPH);
			//int[] t = {2,3,5,6,2,12,4,6,3,36,62,2,144,3,531,44,23,234,21};

			List<int[]> obs = new List<int[]>();

			Random rnd = new Random();

			for(int i=0;i<4;i++) {
				obs.Add(Enumerable.Range(1,49).OrderBy(x => rnd.Next()).ToArray());
			}

			hmm.AddNode(new Node());
			hmm.AddNode(new Node());
			hmm.AddNode(new Node());
			hmm.AddNode(new Node());
			
			BaumWelch BW = new BaumWelch();

			// Act
			HMMGraph result = BW.Learn(hmm, obs.ToArray());

			// Assert
			Assert.IsNotNull(result);
			Assert.Inconclusive();	
		}




		[Test()]
		public void TestObject_StateUnderTest_ExpectedOutcome ()
		{
			// Arrange
		
			// Act
			
			// Assert
			Assert.Inconclusive();	
		}

	}
}

