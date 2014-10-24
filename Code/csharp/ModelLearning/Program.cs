using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;

namespace ModelLearning {
    class Program {
        static void Main(string[] args) {

            DataLoader dataLoader = new DataLoader();
            SequenceData trainSequences = dataLoader.LoadSequences(@"Data/1.pautomac.train");
            SequenceData testSequences = dataLoader.LoadSequences(@"Data/1.pautomac.test");
            double[] realProbs = dataLoader.LoadSolutions(@"Data/1.pautomac_solution.txt");

            //trainSequences.AddSequences(testSequences);

            HiddenMarkovModel hmm = new HiddenMarkovModel(20, trainSequences.NumSymbols);
            var teacher = new BaumWelchLearning(hmm) { Tolerance = 0.001, Iterations = 0 };
            double likelyhood = teacher.Run(trainSequences.CloneNoneEmptySequences());
            double score = PautomacEvaluator.Evaluate(hmm, testSequences, realProbs);
            Console.WriteLine("Score: " + score);
            Console.ReadLine();
        }
    }
}
