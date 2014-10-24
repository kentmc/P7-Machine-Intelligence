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

            List<Learner> learners = new List<Learner>() { new Learners.ManfredLearner() };
            List<int> datasets = new List<int>(){1, 2, 3};
            Benchmarker.Run(learners, datasets);
            Console.ReadLine();
        }
    }
}
