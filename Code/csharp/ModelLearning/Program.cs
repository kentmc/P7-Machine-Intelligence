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
                new Learners.KentManfredLearner(5, 0.01),
                new Learners.BaumWelchLearner(20, 0.0001),
                new Learners.UniformLearner()
            };

            while (true) {
                string output_file = ShowInterfaceSelectOutputFile();
                List<Learner> selected_learners = ShowInterfaceSelectLearners(learners);
                if (selected_learners == null)
                    continue;
                List<int> selected_datasets = ShowInterfaceSelectDatasets(10);
                if (selected_datasets == null)
                    continue;
                Benchmarker.Run(selected_learners, selected_datasets, output_file);
                Console.WriteLine("Benchmarking has finished with success!");
                Console.WriteLine("Do another benchmark ? y/n");
                string response = Console.ReadLine();
                if (response.Length > 0 && response.ToLower()[0] == 'y')
                    continue;
                else
                    break;
            }
        }

        static string ShowInterfaceSelectOutputFile() {
            Console.WriteLine("\nEnter name for the output file (default is 'benchmark_result.txt')");
            string response = Console.ReadLine();
            return response == "" ? "benchmark_result.txt" : response;
        }

        static List<int> ShowInterfaceSelectDatasets(int num_datasets) {
            List<int> selected_datasets = new List<int>();
            Console.WriteLine("\nChoose datasets [1-" + (num_datasets) + "] (e.g. '1,5' for 1 and 5):");
            string response = Console.ReadLine();
            foreach (string s in response.Split(',')) {
                int dataset_index;
                if (!Int32.TryParse(s, out dataset_index)) {
                    Console.WriteLine("Could not parse dataset index: '" + s +"'");
                    return null;
                }
                selected_datasets.Add(dataset_index);
                if (dataset_index <= 0 || dataset_index > num_datasets) {
                    Console.WriteLine("Dataset must be within range [1-"+num_datasets+"], received: " + response);
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
            Console.WriteLine("\nChoose Learners: (e.g. '0,2') for 0 and 2");
            string response = Console.ReadLine();
            foreach (string s in response.Split(',')) {
                int learner_index;
                if (!Int32.TryParse(s, out learner_index)) {
                    Console.WriteLine("Could not parse learner index: '" + s +"'");
                    return null;
                }
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
