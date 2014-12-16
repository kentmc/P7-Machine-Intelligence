using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelLearning;
using ModelLearning.Learners;
using System.Linq;
using System.Collections.Generic;

namespace PadawanTests
{
    [TestClass]
    public class PadawanLearnerTest
    {

        [TestMethod]
        public void isUniform_uniformDist_true()
        {
            // Arrange
            const double THRESHOLD = 0.01;

            PadawanLearner pwl = new PadawanLearner();
            double[] probs = { 0.2, 0.2, 0.2, 0.2, 0.2 };
            // Act
            bool result = pwl.isUniform(probs, THRESHOLD);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void isUniform_nonuniformDist_false()
        {
            // Arrange
            const double THRESHOLD = 0.01;

            PadawanLearner pwl = new PadawanLearner();
            double[] probs = { 0.2, 0.4, 0.1, 0.3 };

            // Act
            bool result = pwl.isUniform(probs, THRESHOLD);


            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Learn_3statefullyconnectedgraph_aHMMfittedtothedata()
        {
            // Arrange
            int[] symbols = Enumerable.Range(0, 41).ToArray();
            List<int[]> obs = new List<int[]>();
            Random rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                obs.Add(symbols.OrderBy(x => rnd.Next()).ToArray());
            }

            SequenceData sd = new SequenceData(0);
            sd.AddSequences(obs);
            sd.SaveAddedSequences();
            PadawanLearner pwl = new PadawanLearner();
            pwl.Initialise(null, 0);
            // Act
            pwl.Learn(sd, null, null);
            //pwl.Learn(sd);
            double result = pwl.CalculateProbability(obs[2]);

            // Assert
            Assert.IsTrue(0.9 < result && result < 1.0);
        }

        [TestMethod]
        public void TestObject_StateUnderTest_ExpectedOutcome()
        {
            // Arrange

            // Act

            // Assert
            Assert.Inconclusive();
        }
    }
}
