using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Statistics.Models.Markov;
using VBGG_Graph = GG_Graph.GG_Graph;

namespace ModelLearning.Learners
{
    class GGLearner: Learner{

        
        private Random random;

        private double bestLikelihood;
        private int maxExpandAttempts;
        private double finalBWThreshold;
        private int BWiterations;
        private int maxStates;
        private int minNbTransitions;
        public int run;


        public VBGG_Graph gg;
        public HMMGraph GGM;
        public SparseHiddenMarkovModel shmm;
        private System.IO.StreamWriter intermediateOutputFile;
        private string intermediateOutputFileName;


        public GGLearner()
        {
            random = new Random();
            run = 0;
        }

        public override void Initialise(LearnerParameters parameters, int iteration)
        {
            maxStates = (int)parameters.AdditionalParameters["maxStates"];
            maxExpandAttempts = (int)parameters.AdditionalParameters["maxExpandAttempts"];
            BWiterations = (int)parameters.AdditionalParameters["BWiterations"];
            finalBWThreshold = (double)parameters.AdditionalParameters["finalBWThreshold"];
            minNbTransitions = (int)parameters.AdditionalParameters["minNbTransitions"];
        }

        public override double CalculateProbability(int[] sequence, bool log = true)
        {
            if (sequence.Length == 0)
                return (log ? 0.0 : 1.0);
            else
                return shmm.Evaluate(sequence, log);
        }

        public override void Learn(SequenceData trainingData, SequenceData validationData, SequenceData testData) {
            int symb;
            List<Node> all_nodes;
            double[][][] all_transitions;
            double[][][] all_emissions;
            Node new_node;

            //trainingData.AddSequences(validationData);
            symb = trainingData.NumSymbols;
            all_nodes = new List<Node>();

            intermediateOutputFile = new System.IO.StreamWriter(intermediateOutputFileName + (run++) + ".csv");
            intermediateOutputFile.WriteLine("States, Likelihood");

            // Creates a graph using GG_Graph.dll, then feeds it every sequence in trainingData to create an initial model
            gg = new VBGG_Graph(symb);
            for (int i = 0; i < trainingData.GetNonempty().Length; i++)
            {
                gg.AddNewSequence(trainingData.GetNonempty()[i]);
            }

            // -----------------------------------------------------------------
            // Converts into the same type of graph used by the other algorithms
            // -----------------------------------------------------------------

            // Creates as many nodes as there are states in the GG_Graph
            for (int i = 0; i < symb; i++)
            {
                for (int j = 0; j < gg.all_states_by_symbol_produced[i].Length; j++)
                {
                    new_node = new Node();
                    all_nodes.Add(new_node);
                }
            }
            all_transitions = new double[all_nodes.Count][][];
            all_emissions = new double[all_nodes.Count][][];

            // PrepareGraph creates the new transitions and emissions of the new nodes
            // if node n1 could transition to nodes n2 and n3, its new emissions would be the symbols produced by n2 and n3
            PrepareGraph(all_nodes, all_transitions, all_emissions, symb);

            // Construct Graph assigns the transitions and emissions to the new nodes
            ConstructGraph(all_nodes, all_transitions, all_emissions);

            // Merges the nodes whose GG_Graph counterpart was a final and/or initial state
            MergeInitialFinalStates(all_nodes, gg.all_states_by_symbol_produced);

            GGM = new HMMGraph(trainingData.NumSymbols);
            GGM.Nodes = all_nodes;
            GGM.NumSymbols = trainingData.NumSymbols;
            GGM.NormalizeKeepInitial();

            // Merges all the nodes which have fewer transitions than minNbTransitions
            MergeFewTransitionNodes();

            GGM.NormalizeKeepInitial();

            // Uses this initial model for another algorithm - here, the Greedy Extend one
            SplittingNodesAlgorithm(trainingData, validationData, GGM);
            Console.WriteLine("Number of states: " + shmm.NumberOfStates);
        }

        private void PrepareGraph(List<Node> all_nodes, double[][][] all_transitions, double[][][] all_emissions, int symb)
        {
            int index;
            int[][] transitions;
            int[] count;
            List<int> symbols;
            
            index = 0;
            for (int i = 0; i < symb; i++)
            {
                for (int j = 0; j < gg.all_states_by_symbol_produced[i].Length; j++)
                {
                    symbols = new List<int>();
                    count = new int[symb];

                    // Lists all transitions from the j-th state producing symbol i, format: GlobalId, Count
                    transitions = gg.all_states_by_symbol_produced[i][j].transitions_out;
                    if (transitions != null)
                    {
                        all_transitions[index] = new double[transitions.Length][];

                        for (int k = 0; k < transitions.Length; k++)
                        {
                            all_transitions[index][k] = new double[2];
                            all_transitions[index][k][0] = GiveGlobalId(gg.all_states_by_symbol_produced, transitions[k][0], transitions[k][1]);
                            all_transitions[index][k][1] = transitions[k][2];

                            count[transitions[k][0]] = count[transitions[k][0]] + transitions[k][2];

                            if (symbols.Contains(transitions[k][0]) == false)
                                symbols.Add(transitions[k][0]);
                        }

                        // Lists all emissions its node counterpart will have, format: Symbol, Count
                        all_emissions[index] = new double[symbols.Count][];
                        for (int k = 0; k < symbols.Count; k++)
                        {
                            all_emissions[index][k] = new double[2];

                            all_emissions[index][k][0] = symbols[k];
                            all_emissions[index][k][1] = count[symbols[k]];
                        }
                    }

                    index = index + 1;
                }
            }
        }

        private void ConstructGraph(List<Node> all_nodes, double[][][] all_transitions, double[][][] all_emissions)
        {
            int index;
            int i;
            int length;
            double[] t;
            double[] e;
            double[][] transitions_of_current_node;
            double[][] emissions_of_current_node;

            length = all_nodes.Count;

            // Assigns the transitions and emissions to every node
            for (index = 0; index < length; index++)
            {
                transitions_of_current_node = all_transitions[index];
                emissions_of_current_node = all_emissions[index];

                if (transitions_of_current_node == null)
                    continue;

                for (i = 0; i < transitions_of_current_node.Length; i++)
                {
                    t = transitions_of_current_node[i];
                    all_nodes[index].Transitions.Add(all_nodes[(int)t[0]], t[1]);
                }

                for (i = 0; i < emissions_of_current_node.Length; i++)
                {
                    e = emissions_of_current_node[i];
                    all_nodes[index].Emissions.Add((int)e[0], e[1]);
                }
            }
        }

        private void SplittingNodesAlgorithm(SequenceData trainingData, SequenceData validationData, HMMGraph GGM)
        {
            // Starts the Greedy Extend Algorithm
            shmm = SparseHiddenMarkovModel.FromGraph(GGM);
            bestLikelihood = shmm.Evaluate(trainingData.GetAll(), true);
            while (shmm.NumberOfStates < maxStates)
            {
                double last_ll = bestLikelihood;
                WriteLine("Number of states: " + shmm.NumberOfStates);
                for (int i = 0; i < maxExpandAttempts; i++)
                { //number of times to try adding a random node
                    GGM = shmm.ToGraph();
                    RandomlyExtendGraphSparsely(GGM);
                    SparseHiddenMarkovModel hmm = SparseHiddenMarkovModel.FromGraph(GGM);
                    if (BWiterations > 0)
                        hmm.Learn(trainingData.GetNonempty(), validationData.GetNonempty(), 0, BWiterations); //Run the BaumWelch algorithm
                    double likelihood = hmm.Evaluate(validationData.GetAll(), true);
                    if (likelihood > bestLikelihood)
                    {
                        bestLikelihood = likelihood;
                        shmm = hmm;
                        WriteLine("+");
                        break;
                    }
                    else
                    {
                        WriteLine("-");
                    }
                }
                if (last_ll == bestLikelihood)
                { //nothing improved, so stop
                    WriteLine("Likelihood not increased for " + maxExpandAttempts + " attempts");
                    break;
                }
                else
                {
                    WriteLine("Likelihood increased to: " + bestLikelihood);
                    OutputIntermediate();
                }
            }
            WriteLine("Runs Baum Welch last time with the final threshold");
            shmm.Learn(trainingData.GetNonempty(), validationData.GetNonempty(), finalBWThreshold);
            bestLikelihood = shmm.Evaluate(validationData.GetAll(), true);
            WriteLine("Final likelihood: " + bestLikelihood);
            OutputIntermediate();
            intermediateOutputFile.Close();
        }

        public void SetIntermediateOutputFile(string fileName)
        {
            intermediateOutputFileName = fileName;
        }

        private void OutputIntermediate()
        {
            intermediateOutputFile.WriteLine(shmm.NumberOfStates + ", " + bestLikelihood);
        }

        /// <summary>
        /// Extends the graph by adding a new node with random transitions, emissions and initial prabability.
        /// The new node will be connected to Log n random nodes (back and forth) with random transition, emission and initial probabilities.
        /// </summary>
        /// <param name="g"></param>
        private void RandomlyExtendGraphSparsely(HMMGraph g)
        {
            //add a new node
            Node new_node = new Node();
            g.AddNode(new_node);

            //Set random initial probability
            new_node.InitialProbability = random.NextDouble();

            //Set random emission probabilities for the new node
            for (int i = 0; i < g.NumSymbols; i++)
                new_node.SetEmission(i, random.NextDouble());

            //Set random transition probabilities from log n random nodes to the new node (in both directions)
            int out_degree = (int)Math.Ceiling(Math.Log(g.NumNodes));
            List<Node> node_list_copy = g.Nodes.Select(n => n).ToList();
            Utilities.Shuffle(node_list_copy);
            for (int i = 0; i < out_degree; i++)
            {
                node_list_copy[i].SetTransition(new_node, random.NextDouble());
                new_node.SetTransition(node_list_copy[i], random.NextDouble());
            }

            //Normalize graph
            g.NormalizeKeepInitial();
        }

        /// <summary>
        /// Shuffle any (I)List with an extension method based on the Fisher-Yates shuffle :
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void MergeFewTransitionNodes()
        {
            List<Node> nodes_to_merge;
            Node new_node;

            nodes_to_merge = new List<Node>();
            new_node = new Node();

            foreach (Node n in GGM.Nodes)
            {
                if (n.Transitions.Count < minNbTransitions)
                {
                    nodes_to_merge.Add(n);
                }
            }
            if (nodes_to_merge.Count > 1)
            {
                MergeNodes(nodes_to_merge, new_node);

                // Split it once, as we know it needs to be split (due to its very nature)
                SplitNode(GGM, GGM.Nodes.IndexOf(new_node));
            }
        }

        private void MergeInitialFinalStates(List<Node> all_nodes, GG_Graph.State[][] all_states)
        {
            int id;
            double prob;
            Node new_node;
            Node removed_node;
            List<Node> nodes_to_remove;
            List<Dictionary<Node, double>> old_transitions;
            List<Dictionary<int, double>> old_emissions;

            nodes_to_remove = new List<Node>();
            old_transitions = new List<Dictionary<Node, double>>();
            old_emissions = new List<Dictionary<int, double>>();
            new_node = new Node();

            // Creating the new node merging them all
            for (int i = 0; i < all_states.Length; i++)
            {
                for (int j = 0; j < all_states[i].Length; j++)
                {
                    // Checks in the GG_Graph is the state was initial or final
                    if (all_states[i][j].isInitial || all_states[i][j].isFinal)
                    {
                        id = GiveGlobalId(all_states, i, all_states[i][j].id);
                        removed_node = all_nodes[i];
                        nodes_to_remove.Add(removed_node);

                        old_transitions.Add(removed_node.Transitions);
                        old_emissions.Add(removed_node.Emissions);
                    }
                }
            }
            new_node.InitialProbability = 1;
            new_node.Transitions = Merge<Node, double>(old_transitions);
            new_node.Emissions = Merge<int, double>(old_emissions);
            all_nodes.Add(new_node);

            // The transitions to the nodes to be removed are replaced by transitions to the new node
            foreach (Node n in all_nodes)
            {
                foreach (Node r in nodes_to_remove)
                {
                    if (n.Transitions.ContainsKey(r))
                    {
                        prob = n.Transitions[r];
                        n.SetTransition(r, 0);
                        n.SetTransition(new_node, prob);
                    }
                }
            }

            // Nothing is left that links the old nodes to the graph => they can be removed
            foreach (Node r in nodes_to_remove)
            {
                all_nodes.Remove(r);
            }
        }

        private void MergeNodes(List<Node> nodes_to_merge, Node new_node)
        {
            List<Dictionary<Node, double>> old_transitions;
            List<Dictionary<int, double>> old_emissions;

            old_transitions = new List<Dictionary<Node, double>>();
            old_emissions = new List<Dictionary<int, double>>();

            new_node.InitialProbability = 0;

            // Creating the new node merging them all
            for (int i = 0; i < nodes_to_merge.Count; i++)
            {
                old_transitions.Add(nodes_to_merge[i].Transitions);
                old_emissions.Add(nodes_to_merge[i].Emissions);
                new_node.InitialProbability = new_node.InitialProbability + nodes_to_merge[i].InitialProbability;
            }
            new_node.Transitions = Merge<Node, double>(old_transitions);
            new_node.Emissions = Merge<int, double>(old_emissions);
            GGM.Nodes.Add(new_node);

            // The transitions to the nodes to be removed are replaced by transitions to the new node
            foreach (Node n in GGM.Nodes)
            {
                foreach (Node r in nodes_to_merge)
                {
                    if (n.Transitions.ContainsKey(r))
                    {
                        n.SetTransition(new_node, n.Transitions[r]);
                        n.SetTransition(r, 0);
                    }
                }
            }

            // Nothing is left that links the old nodes to the graph => they can be removed
            foreach (Node r in nodes_to_merge)
            {
                GGM.Nodes.Remove(r);
            }
        }

        private void SplitNode(HMMGraph graph, int node)
        {
            Node splitNode = graph.Nodes[node];
            Node newNode = new Node();

            graph.AddNode(newNode);

            newNode.InitialProbability = splitNode.InitialProbability;

            foreach (Node n in graph.Nodes)
            {
                newNode.SetTransition(n, random.NextDouble());
            }

            foreach (int i in splitNode.Emissions.Keys)
            {
                newNode.SetEmission(i, random.NextDouble());
            }

            foreach (Node n in graph.Nodes)
            {
                if (n.Transitions.ContainsKey(splitNode))
                {
                    n.SetTransition(newNode, n.Transitions[splitNode]);
                }
            }

            graph.NormalizeKeepInitial();
        }

        public double LogLikelihood(SparseHiddenMarkovModel hmm, SequenceData evaluationData)
        {
            double loglikelihood = 0;
            for (int i = 0; i < evaluationData.Count; i++)
                loglikelihood += hmm.Evaluate(evaluationData[i], true);
            return loglikelihood;
        }

        // Merges two or more dictionaries
        private Dictionary<TKey, double> Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, double>> dictionaries)
        {
            Dictionary<TKey, double> new_dict;
            
            new_dict = new Dictionary<TKey, double>();
            foreach (var dict in dictionaries)
            {
                foreach (var x in dict)
                {
                    if (new_dict.ContainsKey(x.Key))
                        new_dict[x.Key] = new_dict[x.Key] + x.Value;
                    else
                        new_dict.Add(x.Key, x.Value);
                }
            }

            return new_dict;
        }

        // Only works before merging initial and final states - for obvious reasons
        private int GiveGlobalId(GG_Graph.State[][] all_states, int symb, int id)
        {
            int sum;

            sum = 0;
            for (int i = 0; i < symb; i++)
            {
                sum = sum + all_states[i].Length;
            }
            sum = sum + Array.FindIndex(all_states[symb], sta => (sta.id == id));

            return sum;
        }

        public override string Name() {
            return "GG Learner";
        }

        public override void Save(System.IO.StreamWriter outputWriter, System.IO.StreamWriter csvWriter)
        {
            outputWriter.WriteLine("Number of States: {0}", shmm.NumberOfStates);
            outputWriter.WriteLine("Number of Symbols: {0}", shmm.NumberOfSymbols);
            outputWriter.WriteLine("Max expand attempts: {0}", maxExpandAttempts);
            outputWriter.WriteLine("Final BW threshold: {0}", finalBWThreshold);
            outputWriter.WriteLine("Max states: {0}", maxStates);
            outputWriter.WriteLine("Log likelihood {0}", bestLikelihood);
            outputWriter.WriteLine();

            //bestHMM.Save(outputWriter, csvWriter);
        }
    }
}
