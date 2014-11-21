using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    class LearnerParameters
    {
        public double Threshold { get; private set; }

        public double MinimumThreshold { get; private set; }

        public double MaximumThreshold { get; private set; }

        public double ThresholdStepSize { get; private set; }

        public int NumberOfStates { get; private set; }

        public int MinimumNumberOfStates { get; private set; }

        public int MaximumNumberOfStates { get; private set; }

        public int StateStepSize { get; private set; }

        public LearnerParameters(string learner)
        {
            switch (learner.ToLowerInvariant())
            {
                case "baum welch learner":
                    {
                        Console.WriteLine("Configure Baum Welch Learner:");
                        Console.Write("Convergence threshold: ");
                        Threshold = Double.Parse(Console.ReadLine());

                        Console.Write("Minimum Number of States: ");
                        MinimumNumberOfStates = Int32.Parse(Console.ReadLine());

                        Console.Write("Maximum Number of States: ");
                        MaximumNumberOfStates = Int32.Parse(Console.ReadLine());

                        Console.Write("Step Size: ");
                        StateStepSize = Int32.Parse(Console.ReadLine());
                        break;
                    }
                case "uniform learner":
                    {
                        Console.WriteLine("Uniform Learner needs no configuration.");

                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unable to Configure {0}.", learner);

                        break;
                    }
            }

            Console.WriteLine();
        }
    }
}
