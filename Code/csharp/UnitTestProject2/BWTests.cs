using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelLearning;
using System.Collections.Generic;
using System.Linq;

namespace PadawanTests
{
    [TestClass]
    public class BWTests
    {
        [TestMethod]
        public void Learn_HMMWithRandomDistributedProbs_LearnsAConsistentModelBasedOnData()
        {
            // Arrange
            int[] symbols = Enumerable.Range(1, 42).ToArray();
            List<int[]> obs = new List<int[]>();
            Random rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                obs.Add(symbols.OrderBy(x => rnd.Next()).ToArray());
            }

            HMMGraph hmm = new HMMGraph(symbols.Length);
            hmm.AddNode(new Node());
            hmm.AddNode(new Node());
            hmm.AddNode(new Node());
            hmm.AddNode(new Node());

            foreach (Node n in hmm.Nodes)
            {
                foreach (Node m in hmm.Nodes)
                {
                    n.SetTransition(m, 0.5);
                }

                foreach (int i in symbols)
                {
                    n.SetEmission(i, 0.5);
                }
            }

            hmm.Normalize();
            BaumWelch bw = new BaumWelch(obs.Count, hmm);

            // Act
            for (int i = 0; i < 5; i++)
            {
                bw.Learn(hmm, obs.ToArray());
            }
           
            // Assert
            const double PRECISION = .00000000001;
            foreach (Node n in hmm.Nodes)
            {
                //check transitions
                double sum = 0;
                foreach (Node nb in n.Transitions.Keys)
                {
                    sum += n.Transitions[nb];
                }
                Assert.IsTrue(1.0 - PRECISION < sum && sum < 1.0 + PRECISION);

                sum = 0;
                foreach (int o in n.Emissions.Keys)
                {
                    sum += n.Emissions[o];
                }
                Assert.IsTrue(1.0 - PRECISION < sum && sum < 1.0 + PRECISION);
            }
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
