using System;
using System.Linq;
using System.IO;
using Accord.Statistics.Models.Markov;
using ModelLearning;
using System.Collections.Generic;

namespace ModelLearning.Learners {

    public class PadawanLearner : Learner {
        private const double EMPTY_SEQUENCE_PROBABILITY = 1.0;
        private const double TRANSITION_UNIFORMITY_THRESHOLD = 0.0;
        private const double EMISSION_UNIFORMITY_THRESHOLD = 0.0;
        private const double THRESHOLD = 0.0;
        private const int ITERATIONS = 15;
        private SparseHiddenMarkovModel hmm;

        public override string Name() {
            return "Padawan Learner";
        }

        public override double CalculateProbability(int[] sequence,
                                                    bool logarithm = false) {
            if (sequence.Length == 0) {
                return EMPTY_SEQUENCE_PROBABILITY;
            }
            else {
                return hmm.Evaluate(sequence);
            }
        }

        private HMMGraph Splitstate(Node prime, HMMGraph graph) {
            Random random = new Random();
            Node qPrime = graph.Nodes[0];
            Node q1 = new Node();

            foreach (Node x in graph.Nodes) {

                q1.SetTransition(x, random.NextDouble());
            }

            foreach (int symbol in qPrime.Emissions.Keys) {

                q1.SetEmission(symbol, random.NextDouble());
            }

            q1.InitialProbability = random.NextDouble();

            foreach (Node n in graph.Nodes) {

                n.Transitions[q1] = random.NextDouble();
            }

            q1.SetTransition(q1,random.NextDouble());
            graph.AddNode(q1);
            graph.Normalize();

            return graph;
        }

        private static Node FindQPrime(HMMGraph graph, int[] combinedTrainData) {
            BaumWelch bw = new BaumWelch(combinedTrainData.Length, graph);

            bw.PreCompute(graph, combinedTrainData);

            Node qPrime = graph.Nodes[0];
            double best = 0.0;
            double score = 0.0;

            foreach (Node n in graph.Nodes) {

                score = bw.ComputeGamma(n, graph, combinedTrainData); // relative (unscaled)

                if (score > best) {

                    qPrime = n;
                    best = score;
                }
            }
            return qPrime;
        }

        private void CleanGraph(HMMGraph G) {

            foreach (Node n in G.Nodes) {

                Node[] TKeys = n.Transitions.Keys.ToArray();
                for(int i=0;i< TKeys.Length;i++) {

                    Node key = TKeys[i];
                    n.SetTransition(key, n.Transitions[key]);
                }

                int[] EKeys = n.Emissions.Keys.ToArray();
                for (int i = 0; i < EKeys.Length; i++) {

                    int key = EKeys[i];
                    n.SetEmission(key, n.Emissions[key]);
                }
            }
        }

        public override void Learn(SequenceData trainingData,
                SequenceData validationData, SequenceData testData) {
       
            #region Junk
            //hmm.Learn(trainingData.GetNonempty(), 1);

            //foreach (int[] O in trainingData.GetAll()) {
            //    // 1. convert to hmm to graph model.
            //    HMMGraph hmmGraph = ModelConverter.HMM2Graph(hmm);

            //    // 2. find argmax gamma
            //    BaumWelch bw = new BaumWelch(O.Length, hmmGraph);

            //    //Node qPrime = (from n in hmmGraph.Nodes
            //    //               where hmmGraph.Nodes.TrueForAll(x => bw.ComputeGamma(n,
            //    //                   hmmGraph, O) > bw.ComputeGamma(x, hmmGraph, O))
            //    //               select n).Single();

            //    Node qPrime = (from n in hmmGraph.Nodes
            //                   where hmmGraph.Nodes.TrueForAll(x
            //                       => bw.ComputeGamma(n, hmmGraph, O) >= bw.ComputeGamma(x, hmmGraph, O))
            //                   select n).First();

            //    // 3. split node if transition or emission probs 
            //    // are above uniformity threshold. 
            //    double[] transValues = qPrime.Transitions.Values.ToArray();
            //    double[] emissionValues = qPrime.Emissions.Values.ToArray();

            //    if (!isUniform(transValues, TRANSITION_UNIFORMITY_THRESHOLD)
            //        || !isUniform(emissionValues, EMISSION_UNIFORMITY_THRESHOLD)) {
            //        // 4. assign new probs and normalize.

            //        Node q1 = new Node();
            //        Node q2 = new Node();

            //        if (!isUniform(transValues, TRANSITION_UNIFORMITY_THRESHOLD)) {
            //            AssignTransitions(qPrime, q1, q2);
            //        }

            //        if (!isUniform(emissionValues, EMISSION_UNIFORMITY_THRESHOLD)) {
            //            AssignEmissions(qPrime, q1, q2);
            //        }

            //        AssignIncomingTransitions(qPrime, q1, q2, hmmGraph);

            //        q1.InitialProbability = qPrime.InitialProbability / 2;
            //        q2.InitialProbability = qPrime.InitialProbability / 2;

            //        hmmGraph.AddNode(q1);
            //        hmmGraph.AddNode(q2);
            //        hmmGraph.RemoveNode(qPrime);
            //    }
            //    // 5. convert graph model back to hmm
            //    //hmmGraph.Normalize();
            //    hmm = ModelConverter.Graph2HMM(hmmGraph);

            //    // 6. ReLearn model using BW.
            //    hmm.Learn(trainingData.GetAll(), ITERATIONS);
            //}

            #endregion

            HMMGraph graph = hmm.ToGraph();
            CleanGraph(graph);
            Random rnd = new Random();

            List<int> cList = new List<int>();
            foreach (int[] a in trainingData.GetAll()) {

                cList.AddRange(a);
            }
            int[] combinedTrainData = cList.ToArray();

            // Run iterations.
            for (int i = 0; i < 20; i++) {

                graph = hmm.ToGraph();

                Node qPrime = FindQPrime(graph, combinedTrainData);

                graph = Splitstate(qPrime, graph);

                hmm = SparseHiddenMarkovModel.FromGraph(graph);
                hmm.Learn(trainingData.GetAll(), THRESHOLD, ITERATIONS);
            }
            hmm = SparseHiddenMarkovModel.FromGraph(graph);
        }

        // Assign incoming transitions to qPrime between q1 and q2
        private void AssignIncomingTransitions(Node qPrime, Node q1, Node q2, HMMGraph graph) {

            #region Junk
            //Dictionary<Node, double> trans = new Dictionary<Node, double>();

            //foreach (Node n in graph.Nodes)
            //{
            //    Dictionary<Node, double> trs = n.Transitions;

            //    foreach (KeyValuePair<Node, double> kv in trs)
            //    {
            //        trans.Add(kv.Key, kv.Value);
            //    }
            //}

            ////Dictionary<Node,double> rTrans = (from t in trans
            ////                                  where t.Key == qPrime
            ////                                  select t).ToDictionary(x => x.Key, x => x.Value);

            //List<KeyValuePair<Node,double>> rTrans = (from t in trans
            //                                          where t.Key == qPrime
            //                                          select t).ToList();


            //var trans = (from n in graph.Nodes
            //            select n.Transitions.ToList()).SelectMany(x => x);

            //var rTrans = (from kv in trans
            //              where kv.Key == qPrime
            //              select kv).ToList<KeyValuePair<Node,double>>();

            //foreach (KeyValuePair<Node, double> kv in rTrans) {

            //    if (rTrans.IndexOf(kv) < rTrans.Count / 2) {

            //        kv.Key = q1;
            //    }
            //    else {

            //        kv.Key = q2;
            //    }

            //(rTrans.IndexOf(kv) < rTrans.Count / 2) ? (kv.Key = q1) : (kv.Key = q2);

            #endregion

            int tCount = 0;

            foreach (Node n in graph.Nodes) {

                if (n.Transitions.ContainsKey(qPrime)) {
                    tCount++;
                }
            }

            int q1Count = 0;

            foreach (Node n in graph.Nodes) {

                if (n.Transitions.ContainsKey(qPrime)) {

                    double prob = n.Transitions[qPrime];
                    n.SetTransition(qPrime, 0);

                    if (q1Count < tCount / 2) {

                        n.SetTransition(q1, prob);
                    }
                    else {

                        n.SetTransition(q2, prob);
                    }
                }
            }
        }

        private void AssignTransitions(Node qPrime, Node q1, Node q2) {
            double sum = qPrime.Transitions.Values.Sum();

            var trans = (from tr in qPrime.Transitions
                         select tr).OrderBy(tra => tra.Value).ToList();

            while (q1.Transitions.Values.Sum() < sum / 2) {
                q1.Transitions.Add(trans[0].Key, trans[0].Value);
                trans.RemoveAt(0);
            }

            q2.Transitions = trans.ToDictionary(kv => kv.Key, kv => kv.Value);

            //q1.SetTransition(q1, 0.1);
            //q2.SetTransition(q2, 0.1);
        }

        private void AssignEmissions(Node qPrime, Node q1, Node q2) {
            double sum = qPrime.Emissions.Values.Sum();

            var emissions = (from e in qPrime.Emissions
                             select e).OrderBy(em => em.Value).ToList();

            while (q1.Emissions.Values.Sum() < sum / 2) {
                q1.Emissions.Add(emissions[0].Key, emissions[0].Value);
                emissions.RemoveAt(0);
            }

            q2.Emissions = emissions.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        // returns true if the probs have a uniform dist.
        public bool isUniform(double[] probDist, double threshold) {
            var probs = (from p in probDist
                         where p > 0.0
                         select p);

            double Zavg = probs.Sum() / probs.Count();
            double AbsErr = probs.Sum(p => Math.Abs(p - Zavg));

            return AbsErr < threshold;
        }

        private void RunBW() {

            throw new NotImplementedException();
        }

        public override void Initialise(LearnerParameters parameters,
                int iteration) {

            const int NUM_SYMBOLS = 42;
            HMMGraph graph = new HMMGraph(NUM_SYMBOLS);
            graph.AddNode(new Node());
            graph.AddNode(new Node());
            graph.AddNode(new Node());

            foreach (Node n in graph.Nodes) {
                foreach (Node m in graph.Nodes) {
                    n.SetTransition(m, 0.5);
                }

                for (int i = 0; i < NUM_SYMBOLS; i++) {
                    n.SetEmission(i, 0.5);
                }
            }
            graph.Normalize();

            this.hmm = SparseHiddenMarkovModel.FromGraph(graph);
        }

        public override void Save(StreamWriter outputWriter, StreamWriter csvWriter) {

            throw new NotImplementedException();
        }
    }

}

