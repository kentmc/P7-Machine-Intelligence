using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    class LearnerParameters
    {
        public double MinimumThreshold { get; private set; }

        public double MaximumThreshold { get; private set; }

        public double ThresholdStepSize { get; private set; }

        public int NumberOfStates { get; private set; }

        public int MinimumNumberOfStates { get; private set; }

        public int MaximumNumberOfStates { get; private set; }

        public int StateStepSize { get; private set; }

        public Dictionary<string, object> AdditionalParameters { get; private set; }

        public LearnerParameters(string learner)
        {
            AdditionalParameters = new Dictionary<string, object>();

            switch (learner.ToLowerInvariant())
            {
                case "baum welch learner":
                    {
                        Console.WriteLine("Configure Baum-Welch Learner:");
                        Console.Write("Convergence threshold: ");
                        AdditionalParameters.Add("threshold", Double.Parse(Console.ReadLine()));

                        goto case "DEF_STATES";
                    }
                case "jaeger learner":
                    {
                        Console.WriteLine("Configure Greedy State Splitting Learner:");
                        Console.Write("Epsilon: ");
                        AdditionalParameters.Add("epsilon", Double.Parse(Console.ReadLine()));

                        goto case "DEF_THRESHOLD";
                    }
                case "jlearner":
                    {
                        Console.WriteLine("Configure JLearner:");

                        goto case "DEF_THRESHOLD";
                    }
                case "kentmanfredlearner":
                    {
                        Console.WriteLine("Configure Greedy Extend Learner:");

                        Console.Write("Alpha: ");
                        AdditionalParameters.Add("alpha", Double.Parse(Console.ReadLine()));

                        Console.Write("Beta: ");
                        AdditionalParameters.Add("beta", Double.Parse(Console.ReadLine()));

                        goto case "DEF_THRESHOLD";
                    }
                case "sparse baum welch learner":
                    {
                        Console.WriteLine("Configure Sparse Baum-Welch Learner:");
                        Console.Write("Convergence threshold: ");
                        AdditionalParameters.Add("threshold", Double.Parse(Console.ReadLine()));

                        goto case "DEF_STATES";
                    }
                case "uniform learner":
                    {
                        Console.WriteLine("Uniform Learner needs no configuration.");

                        break;
                    }
                case "DEF_STATES":
                    {
                        Console.Write("Minimum Number of States: ");
                        MinimumNumberOfStates = Int32.Parse(Console.ReadLine());

                        Console.Write("Maximum Number of States: ");
                        MaximumNumberOfStates = Int32.Parse(Console.ReadLine());

                        Console.Write("Step Size: ");
                        StateStepSize = Int32.Parse(Console.ReadLine());

                        break;
                    }
                case "DEF_THRESHOLD":
                    {
                        Console.Write("Minimum Threshold: ");
                        MinimumThreshold = Double.Parse(Console.ReadLine());

                        Console.Write("Maximum Threshold: ");
                        MaximumThreshold = Double.Parse(Console.ReadLine());

                        Console.Write("Step Size: ");
                        ThresholdStepSize = Double.Parse(Console.ReadLine());

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
