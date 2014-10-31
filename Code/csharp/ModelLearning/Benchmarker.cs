using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ModelLearning {
    static class Benchmarker {
        public static void Run(IEnumerable<Learner> learners, IEnumerable<int> datasets, string output_file) {
            System.IO.StreamWriter file = new System.IO.StreamWriter(output_file);
            file.Write("Dataset");
            foreach (Learner l in learners) {
                file.Write(", " + l.Name() + " score");
                file.Write(", " + l.Name() + " time");
            }

            foreach (int i in datasets) {
                file.WriteLine();
                file.Write(i);
                Console.WriteLine("Benchmarking dataset "+i);
                SequenceData trainSequences = DataLoader.LoadSequences(@"Data/" + i + ".pautomac.train");
                SequenceData testSequences = DataLoader.LoadSequences(@"Data/" + i + ".pautomac.test");
                double[] solutions = DataLoader.LoadSolutions(@"Data/" + i + ".pautomac_solution.txt");
                Console.WriteLine("Loading data");
                foreach (Learner learner in learners){
                    Console.WriteLine(learner.Name() + " is learning a model");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    learner.Learn(trainSequences, testSequences);
                    sw.Stop();
                    Console.WriteLine("Evaluating learner");
                    double score = PautomacEvaluator.Evaluate(learner, testSequences, solutions);
                    Console.WriteLine(learner.Name() + " score: " + String.Format("{0:0.000}", score));
                    file.Write(", " + String.Format("{0:0.000}", score));
                    file.Write(", " + (int)sw.Elapsed.TotalSeconds);
                }
            }
            file.Close();
        }
    }
}
