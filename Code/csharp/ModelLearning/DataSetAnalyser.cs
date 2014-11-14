using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModelLearning {

    /// <summary>
    /// Returns the number of states in the model file of the given data set
    /// </summary>
    public static class DataSetAnalyser {
        public static int CountStates(int dataset) {

            HashSet<int> states = new HashSet<int>();

            System.IO.StreamReader input = new System.IO.StreamReader("Data/"+dataset+".pautomac_model.txt");
            string content = input.ReadToEnd();
            input.Close();

            //look in transitions
            MatchCollection mc = Regex.Matches(content, @"([0-9]+),[0-9]+,([0-9]+)\)");
            foreach (Match match in mc) {
                states.Add(Int32.Parse(match.Groups[1].Value));
                states.Add(Int32.Parse(match.Groups[2].Value));
            }

            //look in initial states
            string inital_states_content = content.Split('F')[0];
            mc = Regex.Matches(inital_states_content, @"\s\((\d+)\)");
            foreach (Match match in mc) {
                states.Add(Int32.Parse(match.Groups[1].Value));
            }

            return states.Count;
        }

        /// <summary>
        /// Returns the number of transitions in the model file of the given data set
        /// </summary>
        public static int CountTransitions(int dataset) {
            HashSet<string> transitions = new HashSet<string>();

            System.IO.StreamReader input = new System.IO.StreamReader("Data/" + dataset + ".pautomac_model.txt");
            string content = input.ReadToEnd();
            input.Close();

            //find all transitions
            MatchCollection mc = Regex.Matches(content, @"([0-9]+),[0-9]+,([0-9]+)\)");
            foreach (Match match in mc) {
                transitions.Add(match.Groups[1]+","+match.Groups[2]);
            }
            return transitions.Count;
        }
    }
}
