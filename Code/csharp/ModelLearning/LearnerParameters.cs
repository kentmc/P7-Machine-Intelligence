using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning
{
    class LearnerParameters
    {
        public bool RunningParameterDecimal { get; private set; }
        public string RunningParameterName { get; private set; }

        public double Minimum{ get; private set; }

        public double Maximum { get; private set; }

        public double StepSize { get; private set; }

        public Dictionary<string, object> AdditionalParameters { get; private set; }

        public LearnerParameters(string learner)
        {
            AdditionalParameters = new Dictionary<string, object>();

            switch (learner.ToLowerInvariant())
            {
                case "baum welch learner":
                    {
                        RunningParameterDecimal = false;
                        RunningParameterName = "States";

                        Console.WriteLine("Configure Baum-Welch Learner:");
                        Console.Write("Convergence threshold: ");
                        AdditionalParameters.Add("threshold", Double.Parse(Console.ReadLine()));

                        Console.WriteLine("Number of States:");
                        goto case "DEF_RUNNING_PARAM";
                    }
                case "greedy state splitter":
                    {
                        RunningParameterDecimal = true;
                        RunningParameterName = "Threshold";

                        Console.WriteLine("Configure Greedy State Splitting Learner:");
                        Console.Write("Convergence Threshold: ");
                        AdditionalParameters.Add("threshold", Double.Parse(Console.ReadLine()));

                        Console.Write("Epsilon: ");
                        AdditionalParameters.Add("epsilon", Double.Parse(Console.ReadLine()));

                        Console.WriteLine("Define Threshold Range:");
                        goto case "DEF_RUNNING_PARAM";
                    }
                case "jlearner":
                    {
                        Console.WriteLine("Configure JLearner:");

                        goto case "DEF_RUNNING_PARAM";
                    }
                case "greedyextendlearner":
                    {
                        Console.WriteLine("Configure Greedy Extend Learner:");

                        Console.Write("Intermediate BW iterations:");
                        AdditionalParameters.Add("BWiterations", Int32.Parse(Console.ReadLine()));

                        Console.Write("Final BW threshold:");
                        AdditionalParameters.Add("finalBWThreshold", Double.Parse(Console.ReadLine()));

                        Console.Write("Max expand attempts:");
                        AdditionalParameters.Add("maxExpandAttempts", int.Parse(Console.ReadLine()));

                        Console.Write("Maximum number of states:");
                        AdditionalParameters.Add("maxStates", Int32.Parse(Console.ReadLine()));
                        break;
                    }
                case "sparse baum welch learner":
                    {
                        RunningParameterDecimal = false;
                        RunningParameterName = "States";

                        Console.WriteLine("Configure Sparse Baum-Welch Learner:");
                        Console.Write("Convergence threshold: ");
                        AdditionalParameters.Add("threshold", Double.Parse(Console.ReadLine()));

                        Console.WriteLine("Number of States:");
                        goto case "DEF_RUNNING_PARAM";
                    }
                case "gg learner":
                    {
                        Console.WriteLine("Configure GG Learner:");

                        Console.Write("Minimum initial number or transitions:");
                        AdditionalParameters.Add("minNbTransitions", Int32.Parse(Console.ReadLine()));

                        Console.Write("Intermediate BW iterations:");
                        AdditionalParameters.Add("BWiterations", Int32.Parse(Console.ReadLine()));

                        Console.Write("Final BW threshold:");
                        AdditionalParameters.Add("finalBWThreshold", Double.Parse(Console.ReadLine()));

                        Console.Write("Max expand attempts:");
                        AdditionalParameters.Add("maxExpandAttempts", int.Parse(Console.ReadLine()));

                        Console.Write("Maximum number of states:");
                        AdditionalParameters.Add("maxStates", Int32.Parse(Console.ReadLine()));
                        break;
                    }
                case "uniform learner":
                    {
                        Console.WriteLine("Uniform Learner needs no configuration.");

                        break;
                    }
                case "DEF_RUNNING_PARAM":
                    {
                        Console.Write("Minimum: ");
                        Minimum = Double.Parse(Console.ReadLine());

                        Console.Write("Maximum: ");
                        Maximum = Double.Parse(Console.ReadLine());

                        Console.Write("Step Size: ");
                        StepSize = Double.Parse(Console.ReadLine());

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
