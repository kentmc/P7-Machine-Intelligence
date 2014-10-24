using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics.Models.Markov;

namespace ModelLearning {
    static class ModelConverter {
        public static HMMGraph HMM2Graph(HiddenMarkovModel hmm){
            HMMGraph g = new HMMGraph(hmm.Symbols, hmm.States);


            //add initial probabilities
            for (int i = 0; i < hmm.States; i++)
                g.SetInitial(i, hmm.Probabilities[i]);

            //add emission probabilities
            for (int i = 0; i < hmm.States; i++)
                for (int j = 0; j < hmm.Symbols; j++)
                    g.SetEmission(i, j, hmm.Emissions[i,j]);

            //add transitions
            double[,] transitions = hmm.Transitions;
            for (int i = 0; i < transitions.GetLength(0); i++) {
                for (int j = 0; j < transitions.GetLength(1); j++)
                    if (transitions[i, j] == 0)
                        g.SetTransition(i, j, transitions[i, j]);
            }
            return g;

            //add emissions
            double[,] emissions = hmm.Emissions;


        }

        public static HiddenMarkovModel Graph2HMM(HMMGraph g) {
            throw new Exception("Not implemented");
        }
    }
}
