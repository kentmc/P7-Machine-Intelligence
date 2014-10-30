using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;

namespace ModelLearning {
    class Program {
        static void Main(string[] args) {

            List<Learner> learners = new List<Learner>() { 
                new Learners.ManfredLearner(20, 10),
                new Learners.BaumWelchLearner(20, 0.0001),
                new Learners.UniformLearner()
            };

            while (true) {
                List<Learner> selected_learners = ShowInterfaceSelectLearners(learners);
                if (selected_learners == null)
                    continue;
                List<int> selected_datasets = ShowInterfaceSelectDatasets(10);
                if (selected_datasets == null)
                    continue;
                Benchmarker.Run(selected_learners, selected_datasets, "benchmark_result.txt");
                Console.WriteLine("Benchmarking has finished with success!");
                Console.WriteLine("Do another benchmark ? y/n");
                string response = Console.ReadLine();
                if (response.Length > 0 && response.ToLower()[0] == 'y')
                    continue;
                else
                    break;
            }
        }

        static List<int> ShowInterfaceSelectDatasets(int num_datasets) {
            List<int> selected_datasets = new List<int>();
            Console.WriteLine("\nChoose datasets [0-" + (num_datasets - 1) + "] (e.g. '02' for 0 and 2):");
            string response = Console.ReadLine();
            for (int i = 0; i < response.Length; i++) {
                int dataset_index = Int32.Parse(response[i].ToString());
                selected_datasets.Add(dataset_index);
                if (dataset_index < 0 || dataset_index >= num_datasets) {
                    Console.WriteLine("Dataset must be within range [0-9], received: " + response);
                    return null;
                }
            }
            return selected_datasets;
        }

        static List<Learner> ShowInterfaceSelectLearners(List<Learner> learners) {
            List<Learner> selected_learners = new List<Learner>();
            for (int i = 0; i < learners.Count; i++) {
                Console.WriteLine(i + ": " + learners[i].Name());
            }
            Console.WriteLine("Choose Learners: (e.g. '02') for 0 and 2");
            string response = Console.ReadLine();
            for (int i = 0; i < response.Length; i++) {
                int learner_index = Int32.Parse(response[i].ToString());
                if (learner_index < 0 || learner_index >= learners.Count) {
                    Console.WriteLine("Learner index must be within range [0-" + learners.Count + "], received " + learner_index);
                    return null;
                }
                selected_learners.Add(learners[learner_index]);
            }
            return selected_learners;
        }
    }
}
