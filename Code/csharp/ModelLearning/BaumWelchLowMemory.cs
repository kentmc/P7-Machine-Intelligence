// Accord Statistics Library
// Accord.NET framework
// http://www.crsouza.com
//
// Copyright © César Souza, 2009-2011
// Copyright © Guilherme Pedroso, 2009
//

namespace Accord.Statistics.Models.Markov {
    using System;

    /// <summary>
    ///   Hidden Markov Model.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   Hidden Markov Models (HMM) are stochastic methods to model temporal and sequence
    ///   data. They are especially known for their application in temporal pattern recognition
    ///   such as speech, handwriting, gesture recognition, part-of-speech tagging, musical
    ///   score following, partial discharges and bioinformatics.</para>
    /// <para>
    ///   Dynamical systems of discrete nature assumed to be governed by a Markov chain emits
    ///   a sequence of observable outputs. Under the Markov assumption, it is also assumed that
    ///   the latest output depends only on the current state of the system. Such states are often
    ///   not known from the observer when only the output values are observable.</para>
    ///   
    /// <para>
    ///   Hidden Markov Models attempt to model such systems and allow, among other things,
    ///   <list type="number">
    ///     <item><description>
    ///       To infer the most likely sequence of states that produced a given output sequence,</description></item>
    ///     <item><description>
    ///       Infer which will be the most likely next state (and thus predicting the next output),</description></item>
    ///     <item><description>
    ///       Calculate the probability that a given sequence of outputs originated from the system
    ///       (allowing the use of hidden Markov models for sequence classification).</description></item>
    ///     </list></para>
    ///     
    /// <para>     
    ///   The “hidden” in Hidden Markov Models comes from the fact that the observer does not
    ///   know in which state the system may be in, but has only a probabilistic insight on where
    ///   it should be.</para>
    ///   
    /// <para>
    ///   References:
    ///   <list type="bullet">
    ///     <item><description>
    ///       http://en.wikipedia.org/wiki/Hidden_Markov_model</description></item>
    ///     <item><description>
    ///       http://www.shokhirev.com/nikolai/abc/alg/hmm/hmm.html</description></item>
    ///     <item><description>
    ///       P396-397 “Spoken Language Processing” by X. Huang </description></item>
    ///     <item><description>
    ///       Dawei Shen. Some mathematics for HMMs, 2008. Available in:
    ///       http://courses.media.mit.edu/2010fall/mas622j/ProblemSets/ps4/tutorial.pdf</description></item>
    ///     <item><description>
    ///       http://www.stanford.edu/class/cs262/presentations/lecture7.pdf</description></item>
    ///     <item><description>
    ///       http://cs.oberlin.edu/~jdonalds/333/lecture11.html</description></item>
    ///   </list></para>
    /// </remarks>
    /// 
    /// <example>
    ///   <code>
    ///   // We will try to create a Hidden Markov Model which
    ///   //  can detect if a given sequence starts with a zero
    ///   //  and has any number of ones after that.
    ///   int[][] sequences = new int[][] 
    ///   {
    ///       new int[] { 0,1,1,1,1,0,1,1,1,1 },
    ///       new int[] { 0,1,1,1,0,1,1,1,1,1 },
    ///       new int[] { 0,1,1,1,1,1,1,1,1,1 },
    ///       new int[] { 0,1,1,1,1,1         },
    ///       new int[] { 0,1,1,1,1,1,1       },
    ///       new int[] { 0,1,1,1,1,1,1,1,1,1 },
    ///       new int[] { 0,1,1,1,1,1,1,1,1,1 },
    ///   };
    ///   
    ///   // Creates a new Hidden Markov Model with 3 states for
    ///   //  an output alphabet of two characters (zero and one)
    ///   HiddenMarkovModel hmm = new HiddenMarkovModel(2, 3);
    ///   
    ///   // Try to fit the model to the data until the difference in
    ///   //  the average log-likelihood changes only by as little as 0.0001
    ///   hmm.Learn(sequences, 0.0001);
    ///   
    ///   // Calculate the probability that the given
    ///   //  sequences originated from the model
    ///   double l1 = hmm.Evaluate(new int[] { 0, 1 });        // 0.9999  
    ///   double l2 = hmm.Evaluate(new int[] { 0, 1, 1, 1 });  // 0.9166
    ///   
    ///   // Sequences which do not start with zero have much lesser probability.
    ///   double l3 = hmm.Evaluate(new int[] { 1, 1 });        // 0.0000
    ///   double l4 = hmm.Evaluate(new int[] { 1, 0, 0, 0 });  // 0.0000
    ///   </code>
    /// </example>
    /// 
    [Serializable]
    public class HiddenMarkovModelLowMemory {

        // Model is defined as M = (A, B, pi)
        private double[,] B; // Emission probabilities
        private double[,] A; // Transition probabilities
        private double[] pi; // Initial state probabilities

        private int symbols; //Size of vocabulary
        private int states;  // number of states
        private object tag;


        //---------------------------------------------


        #region Constructors
        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="symbols">The number of output symbols used for this model.</param>
        /// <param name="probabilities">The initial state probabilities for this model.</param>
        public HiddenMarkovModelLowMemory(int symbols, params double[] probabilities)
            : this(null, probabilities, null, HiddenMarkovModelType.Ergodic) {
            if (symbols <= 0) {
                throw new ArgumentOutOfRangeException("symbols",
                    "Number of symbols should be higher than zero.");
            }

            this.symbols = symbols;
            this.B = new double[States, symbols];

            // Initialize B with uniform probabilities
            for (int i = 0; i < States; i++)
                for (int j = 0; j < symbols; j++)
                    B[i, j] = 1.0 / symbols;
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="transitions">The transitions matrix A for this model.</param>
        /// <param name="emissions">The emissions matrix B for this model.</param>
        /// <param name="probabilities">The initial state probabilities for this model.</param>
        public HiddenMarkovModelLowMemory(double[,] transitions, double[,] emissions, double[] probabilities)
            : this(transitions, probabilities, null, HiddenMarkovModelType.Ergodic) {
            this.symbols = emissions.GetLength(1);
            this.B = emissions;
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="symbols">The number of output symbols used for this model.</param>
        /// <param name="states">The number of states for this model.</param>
        public HiddenMarkovModelLowMemory(int symbols, int states)
            : this(symbols, states, HiddenMarkovModelType.Ergodic) {
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        /// <param name="symbols">The number of output symbols used for this model.</param>
        /// <param name="states">The number of states for this model.</param>
        /// <param name="type">The topology which should be used by this model.</param>
        public HiddenMarkovModelLowMemory(int symbols, int states, HiddenMarkovModelType type)
            : this(null, null, states, type) {
            if (symbols <= 0) {
                throw new ArgumentOutOfRangeException("symbols",
                    "Number of symbols should be higher than zero.");
            }

            this.symbols = symbols;
            this.B = new double[states, symbols];

            // Initialize B with uniform probabilities
            for (int i = 0; i < states; i++)
                for (int j = 0; j < symbols; j++)
                    B[i, j] = 1.0 / symbols;
        }

        /// <summary>
        ///   Constructs a new Hidden Markov Model.
        /// </summary>
        private HiddenMarkovModelLowMemory(double[,] transitions, double[] probabilities, int? states, HiddenMarkovModelType type) {
            // Automatically determine missing parameters

            #region Number of states N
            if (states != null) {
                if (states <= 0) {
                    throw new ArgumentOutOfRangeException("states",
                        "Number of states should be higher than zero.");
                }
            }
            else {
                if (probabilities != null) {
                    states = probabilities.Length;
                }
                else if (transitions != null) {
                    states = transitions.GetLength(0);
                }
                else {
                    throw new ArgumentException("Number of states could not be determined.",
                        "states");
                }
            }

            int n = states.Value;
            this.states = n;
            #endregion

            #region Transitions Matrix A
            if (transitions != null) {
                if (transitions.GetLength(0) != states)
                    throw new ArgumentException(
                        "Transition matrix should have the same dimensions as the number of model states.",
                        "transitions");

                if (transitions.GetLength(0) != transitions.GetLength(1))
                    throw new ArgumentException("Transition matrix should be square.", "transitions");
            }
            else {


                if (type == HiddenMarkovModelType.Ergodic) {
                    // Create A using uniform distribution
                    transitions = new double[n, n];
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < n; j++)
                            transitions[i, j] = 1.0 / n;
                }
                else {
                    // Create A using uniform distribution,
                    //   without allowing backward transitions.
                    transitions = new double[n, n];
                    for (int i = 0; i < n; i++)
                        for (int j = i; j < n; j++)
                            transitions[i, j] = 1.0 / (n - i);
                }
            }

            this.A = transitions;
            #endregion

            #region Initial Probabilities pi
            if (probabilities != null) {
                if (probabilities.Length != n)
                    throw new ArgumentException(
                        "Initial probabilities should have the same length as the number of model states.",
                        "probabilities");
            }
            else {
                // Create pi as left-to-right
                probabilities = new double[n];
                probabilities[0] = 1.0;
            }

            this.pi = probabilities;
            #endregion

        }
        #endregion


        //---------------------------------------------


        #region Public Properties
        /// <summary>
        ///   Gets the number of symbols in the alphabet of this model.
        /// </summary>
        public int Symbols {
            get { return symbols; }
        }

        /// <summary>
        ///   Gets the Emission matrix (B) for this model.
        /// </summary>
        public double[,] Emissions {
            get { return this.B; }
        }

        /// <summary>
        ///   Gets the number of states of this model.
        /// </summary>
        public int States {
            get { return this.states; }
        }

        /// <summary>
        ///   Gets the initial probabilities for this model.
        /// </summary>
        public double[] Probabilities {
            get { return this.pi; }
        }

        /// <summary>
        ///   Gets the Transition matrix (A) for this model.
        /// </summary>
        public double[,] Transitions {
            get { return this.A; }
        }

        /// <summary>
        ///   Gets or sets a user-defined tag.
        /// </summary>
        public object Tag {
            get { return tag; }
            set { tag = value; }
        }
        #endregion


        //---------------------------------------------


        #region Public Methods

        /// <summary>
        ///   Calculates the most likely sequence of hidden states
        ///   that produced the given observation sequence.
        /// </summary>
        /// <remarks>
        ///   Decoding problem. Given the HMM M = (A, B, pi) and  the observation sequence 
        ///   O = {o1,o2, ..., oK}, calculate the most likely sequence of hidden states Si
        ///   that produced this observation sequence O. This can be computed efficiently
        ///   using the Viterbi algorithm.
        /// </remarks>
        /// <param name="observations">A sequence of observations.</param>
        /// <param name="probability">The state optimized probability.</param>
        /// <returns>The sequence of states that most likely produced the sequence.</returns>
        public int[] Decode(int[] observations, out double probability) {
            return Decode(observations, false, out probability);
        }

        /// <summary>
        ///   Calculates the most likely sequence of hidden states
        ///   that produced the given observation sequence.
        /// </summary>
        /// <remarks>
        ///   Decoding problem. Given the HMM M = (A, B, pi) and  the observation sequence 
        ///   O = {o1,o2, ..., oK}, calculate the most likely sequence of hidden states Si
        ///   that produced this observation sequence O. This can be computed efficiently
        ///   using the Viterbi algorithm.
        /// </remarks>
        /// <param name="observations">A sequence of observations.</param>
        /// <param name="probability">The state optimized probability.</param>
        /// <param name="logarithm">True to return the log-likelihood, false to return
        /// the likelihood. Default is false (default is to return the likelihood).</param>
        /// <returns>The sequence of states that most likely produced the sequence.</returns>
        public int[] Decode(int[] observations, bool logarithm, out double probability) {
            if (observations == null)
                throw new ArgumentNullException("observations");

            if (observations.Length == 0) {
                probability = 0.0;
                return new int[0];
            }


            // Viterbi-forward algorithm.
            int T = observations.Length;
            int states = States;
            int minState;
            double minWeight;
            double weight;

            double[] pi = Probabilities;
            double[,] A = Transitions;

            int[,] s = new int[states, T];
            double[,] a = new double[states, T];


            // Base
            for (int i = 0; i < states; i++)
                a[i, 0] = -Math.Log(pi[i]) - Math.Log(B[i, observations[0]]);

            // Induction
            for (int t = 1; t < T; t++) {
                int observation = observations[t];

                for (int j = 0; j < states; j++) {
                    minState = 0;
                    minWeight = a[0, t - 1] - Math.Log(A[0, j]);

                    for (int i = 1; i < states; i++) {
                        weight = a[i, t - 1] - Math.Log(A[i, j]);

                        if (weight < minWeight) {
                            minState = i;
                            minWeight = weight;
                        }
                    }

                    a[j, t] = minWeight - Math.Log(B[j, observation]);
                    s[j, t] = minState;
                }
            }


            // Find minimum value for time T-1
            minState = 0;
            minWeight = a[0, T - 1];

            for (int i = 1; i < states; i++) {
                if (a[i, T - 1] < minWeight) {
                    minState = i;
                    minWeight = a[i, T - 1];
                }
            }


            // Trackback
            int[] path = new int[T];
            path[T - 1] = minState;

            for (int t = T - 2; t >= 0; t--)
                path[t] = s[path[t + 1], t + 1];


            // Returns the sequence probability as an out parameter
            probability = logarithm ? -minWeight : Math.Exp(-minWeight);

            // Returns the most likely (Viterbi path) for the given sequence
            return path;
        }

        /// <summary>
        ///   Calculates the probability that this model has generated the given sequence.
        /// </summary>
        /// <remarks>
        ///   Evaluation problem. Given the HMM  M = (A, B, pi) and  the observation
        ///   sequence O = {o1, o2, ..., oK}, calculate the probability that model
        ///   M has generated sequence O. This can be computed efficiently using the
        ///   either the Viterbi or the Forward algorithms.
        /// </remarks>
        /// <param name="observations">
        ///   A sequence of observations.
        /// </param>
        /// <returns>
        ///   The probability that the given sequence has been generated by this model.
        /// </returns>
        public double Evaluate(int[] observations) {
            return Evaluate(observations, false);
        }

        /// <summary>
        ///   Calculates the probability that this model has generated the given sequence.
        /// </summary>
        /// <remarks>
        ///   Evaluation problem. Given the HMM  M = (A, B, pi) and  the observation
        ///   sequence O = {o1, o2, ..., oK}, calculate the probability that model
        ///   M has generated sequence O. This can be computed efficiently using the
        ///   either the Viterbi or the Forward algorithms.
        /// </remarks>
        /// <param name="observations">
        ///   A sequence of observations.
        /// </param>
        /// <param name="logarithm">
        ///   True to return the log-likelihood, false to return
        ///   the likelihood. Default is false.
        /// </param>
        /// <returns>
        ///   The probability that the given sequence has been generated by this model.
        /// </returns>
        public double Evaluate(int[] observations, bool logarithm) {
            if (observations == null)
                throw new ArgumentNullException("observations");

            if (observations.Length == 0)
                return 0.0;


            // Forward algorithm
            double likelihood = 0;
            double[] coefficients;

            // Compute forward probabilities
            forward(observations, out coefficients);

            for (int i = 0; i < coefficients.Length; i++)
                likelihood += Math.Log(coefficients[i]);

            // Return the sequence probability
            return logarithm ? likelihood : Math.Exp(likelihood);
        }

        /// <summary>
        /// Returns the log likelihood that the model have generated the given data
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="logarithm">whether the likelihood should be returned as a loglikelihood or just a likelihood</param>
        /// <returns></returns>
        public double Evaluate(int[][] observations, bool logarithm) {
            double loglikelihood = 0;
            for (int i = 0; i < observations.Length; i++)
                loglikelihood += Evaluate(observations[i], true);
            return logarithm ? loglikelihood : Math.Exp(loglikelihood);
        }

        /// <summary>
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// </summary>
        /// <remarks>
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// </remarks>
        /// <param name="observations">
        ///   The sequence of observations to be used to train the model.
        /// </param>
        /// <param name="iterations">
        ///   The maximum number of iterations to be performed by the learning algorithm. If
        ///   specified as zero, the algorithm will learn until convergence of the model average
        ///   likelihood respecting the desired limit.
        /// </param>
        /// <param name="tolerance">
        ///   The likelihood convergence limit L between two iterations of the algorithm. The
        ///   algorithm will stop when the change in the likelihood for two consecutive iterations
        ///   has not changed by more than L percent of the likelihood. If left as zero, the
        ///   algorithm will ignore this parameter and iterates over a number of fixed iterations
        ///   specified by the previous parameter.
        /// </param>
        /// <returns>
        ///   The average log-likelihood for the observations after the model has been trained.
        /// </returns>
        public double Learn(int[] observations, int iterations, double tolerance) {
            return Learn(new int[][] { observations }, iterations, tolerance);
        }

        /// <summary>
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// </summary>
        /// <remarks>
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// </remarks>
        /// <param name="observations">
        ///   The sequence of observations to be used to train the model.
        /// </param>
        /// <param name="iterations">
        ///   The maximum number of iterations to be performed by the learning algorithm. If
        ///   specified as zero, the algorithm will learn until convergence of the model average
        ///   likelihood respecting the desired limit.
        /// </param>
        /// <returns>
        ///   The average log-likelihood for the observations after the model has been trained.
        /// </returns>
        public double Learn(int[] observations, int iterations) {
            return Learn(observations, iterations, 0.0);
        }

        /// <summary>
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// </summary>
        /// <remarks>
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// </remarks>
        /// <param name="observations">
        ///   The sequence of observations to be used to train the model.
        /// </param>
        /// <param name="tolerance">
        ///   The likelihood convergence limit L between two iterations of the algorithm. The
        ///   algorithm will stop when the change in the likelihood for two consecutive iterations
        ///   has not changed by more than L percent of the likelihood. If left as zero, the
        ///   algorithm will ignore this parameter and iterates over a number of fixed iterations
        ///   specified by the previous parameter.
        /// </param>
        /// <returns>
        ///   The average log-likelihood for the observations after the model has been trained.
        /// </returns>
        public double Learn(int[] observations, double tolerance) {
            return Learn(observations, 0, tolerance);
        }

        /// <summary>
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// </summary>
        /// <remarks>
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// </remarks>
        /// <param name="observations">
        ///   An array of observation sequences to be used to train the model.
        /// </param>
        /// <param name="tolerance">
        ///   The likelihood convergence limit L between two iterations of the algorithm. The
        ///   algorithm will stop when the change in the likelihood for two consecutive iterations
        ///   has not changed by more than L percent of the likelihood. If left as zero, the
        ///   algorithm will ignore this parameter and iterates over a number of fixed iterations
        ///   specified by the previous parameter.
        /// </param>
        /// <returns>
        ///   The average log-likelihood for the observations after the model has been trained.
        /// </returns>
        public double Learn(int[][] observations, double tolerance) {
            return Learn(observations, 0, tolerance);
        }

        /// <summary>
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// </summary>
        /// <remarks>
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// </remarks>
        /// <param name="observations">
        ///   An array of observation sequences to be used to train the model.
        /// </param>
        /// <param name="iterations">
        ///   The maximum number of iterations to be performed by the learning algorithm. If
        ///   specified as zero, the algorithm will learn until convergence of the model average
        ///   likelihood respecting the desired limit.
        /// </param>
        /// <returns>
        ///   The average log-likelihood for the observations after the model has been trained.
        /// </returns>
        public double Learn(int[][] observations, int iterations) {
            return Learn(observations, iterations, 0);
        }

        /// <summary>
        ///   Runs the Baum-Welch learning algorithm for hidden Markov models.
        /// </summary>
        /// <remarks>
        ///   Learning problem. Given some training observation sequences O = {o1, o2, ..., oK}
        ///   and general structure of HMM (numbers of hidden and visible states), determine
        ///   HMM parameters M = (A, B, pi) that best fit training data. 
        /// </remarks>
        /// <param name="iterations">
        ///   The maximum number of iterations to be performed by the learning algorithm. If
        ///   specified as zero, the algorithm will learn until convergence of the model average
        ///   likelihood respecting the desired limit.
        /// </param>
        /// <param name="observations">
        ///   An array of observation sequences to be used to train the model.
        /// </param>
        /// <param name="tolerance">
        ///   The likelihood convergence limit L between two iterations of the algorithm. The
        ///   algorithm will stop when the change in the likelihood for two consecutive iterations
        ///   has not changed by more than L percent of the likelihood. If left as zero, the
        ///   algorithm will ignore this parameter and iterates over a number of fixed iterations
        ///   specified by the previous parameter.
        /// </param>
        /// <returns>
        ///   The average log-likelihood for the observations after the model has been trained.
        /// </returns>
        public double Learn(int[][] observations, int iterations, double tolerance) {
            Console.Write("*");
            if (iterations == 0 && tolerance == 0)
                throw new ArgumentException("Iterations and limit cannot be both zero.");

            // Baum-Welch algorithm.

            // The Baum–Welch algorithm is a particular case of a generalized expectation-maximization
            // (GEM) algorithm. It can compute maximum likelihood estimates and posterior mode estimates
            // for the parameters (transition and emission probabilities) of an HMM, when given only
            // emissions as training data.

            // The algorithm has two steps:
            //  - Calculating the forward probability and the backward probability for each HMM state;
            //  - On the basis of this, determining the frequency of the transition-emission pair values
            //    and dividing it by the probability of the entire string. This amounts to calculating
            //    the expected count of the particular transition-emission pair. Each time a particular
            //    transition is found, the value of the quotient of the transition divided by the probability
            //    of the entire string goes up, and this value can then be made the new value of the transition.


            int N = observations.Length;
            int currentIteration = 1;
            bool stop = false;

            double[] pi = Probabilities;
            double[,] A = Transitions;

            // Calculate initial model log-likelihood
            double oldLikelihood = Double.MinValue;
            double newLikelihood = 0;

            do // Until convergence or max iterations is reached
            {
                Console.Write("*");

                double[] newPi = new double[pi.GetLength(0)];
                double[,] newANum = new double[A.GetLength(0), A.GetLength(1)];
                double[,] newADen = new double[A.GetLength(0), A.GetLength(1)];
                double[,] newBNum = new double[B.GetLength(0), B.GetLength(1)];
                double[,] newBDen = new double[B.GetLength(0), B.GetLength(1)];

                // For each sequence in the observations input
                for (int i = 0; i < N; i++) {

                    var sequence = observations[i];
                    int T = sequence.Length;
                    double[] scaling;

                    double[, ,] epsilon = new double[T, States, States]; // also referred as ksi or psi
                    double[,] gamma = new double[T, States];

                    // 1st step - Calculating the forward probability and the
                    //            backward probability for each HMM state.
                    double[,] fwd = forward(observations[i], out scaling);
                    double[,] bwd = backward(observations[i], scaling);


                    // 2nd step - Determining the frequency of the transition-emission pair values
                    //            and dividing it by the probability of the entire string.


                    // Calculate gamma values for next computations
                    for (int t = 0; t < T; t++) {
                        double s = 0;

                        for (int k = 0; k < States; k++)
                            s += gamma[t, k] = fwd[t, k] * bwd[t, k];

                        if (s != 0) // Scaling
                        {
                            for (int k = 0; k < States; k++)
                                gamma[t, k] /= s;
                        }
                    }

                    // Calculate epsilon values for next computations
                    for (int t = 0; t < T - 1; t++) {
                        double s = 0;

                        for (int k = 0; k < States; k++)
                            for (int l = 0; l < States; l++)
                                s += epsilon[t, k, l] = fwd[t, k] * A[k, l] * bwd[t + 1, l] * B[l, sequence[t + 1]];

                        if (s != 0) // Scaling
                        {
                            for (int k = 0; k < States; k++)
                                for (int l = 0; l < States; l++)
                                    epsilon[t, k, l] /= s;
                        }
                    }

                    // Compute log-likelihood for the given sequence
                    for (int t = 0; t < scaling.Length; t++)
                        newLikelihood += Math.Log(scaling[t]);

                    // 3.1 Re-estimation of initial state probabilities 
                    for (int k = 0; k < States; k++)
                        newPi[k] += gamma[0, k];

                    // 3.2 Re-estimation of transition probabilities 
                    for (int g = 0; g < States; g++) {
                        for (int h = 0; h < States; h++) {
                            for (int l = 0; l < T - 1; l++) {
                                newANum[g, h] += epsilon[l, g, h];
                                newADen[g, h] += gamma[l, g];
                            }
                        }
                    }

                    // 3.3 Re-estimation of emission probabilities
                    for (int g = 0; g < States; g++) {
                        for (int h = 0; h < Symbols; h++) {
                            for (int l = 0; l < T; l++) {
                                if (observations[i][l] == h)
                                    newBNum[g, h] += gamma[l, g];
                            }
                            for (int l = 0; l < T; l++)
                                newBDen[g, h] += gamma[l, g];
                        }
                    }
                }


                // Average the likelihood for all sequences
                newLikelihood /= observations.Length;


                // Check if the model has converged or we should stop
                if (checkConvergence(oldLikelihood, newLikelihood,
                    currentIteration, iterations, tolerance)) {
                    stop = true;
                }

                
                // 3. Continue with parameter re-estimation
                currentIteration++;
                oldLikelihood = newLikelihood;
                newLikelihood = 0.0;

                //3.1 Re-estimation of initial probabilities
                for (int i = 0; i < States; i++)
                    pi[i] = newPi[i] / N;

                // 3.2 Re-estimation of transition probabilities 
                for (int i = 0; i < States; i++) {
                    for (int j = 0; j < States; j++) {
                        double den = newADen[i, j];
                        double num = newANum[i, j];
                        A[i, j] = (den != 0) ? num / den : 0.0;
                    }
                }

                // 3.3 Re-estimation of emission probabilities
                for (int i = 0; i < States; i++) {
                    for (int j = 0; j < Symbols; j++) {
                        double den = newBDen[i, j];
                        double num = newBNum[i, j];
                        // avoid locking a parameter in zero.
                        B[i, j] = (num == 0) ? 1e-10 : num / den;
                    }
                }

                

            } while (!stop);


            // Returns the model average log-likelihood
            return newLikelihood;
        }
        #endregion


        //---------------------------------------------


        #region Private Methods
        /// <summary>
        ///   Baum-Welch forward pass (with scaling)
        /// </summary>
        /// <remarks>
        ///   Reference: http://courses.media.mit.edu/2010fall/mas622j/ProblemSets/ps4/tutorial.pdf
        /// </remarks>
        private double[,] forward(int[] observations, out double[] c) {
            int T = observations.Length;
            double[] pi = Probabilities;
            double[,] A = Transitions;

            double[,] fwd = new double[T, States];
            c = new double[T];


            // 1. Initialization
            for (int i = 0; i < States; i++)
                c[0] += fwd[0, i] = pi[i] * B[i, observations[0]];

            if (c[0] != 0) // Scaling
            {
                for (int i = 0; i < States; i++)
                    fwd[0, i] = fwd[0, i] / c[0];
            }


            // 2. Induction
            for (int t = 1; t < T; t++) {
                for (int i = 0; i < States; i++) {
                    double p = B[i, observations[t]];

                    double sum = 0.0;
                    for (int j = 0; j < States; j++)
                        sum += fwd[t - 1, j] * A[j, i];
                    fwd[t, i] = sum * p;

                    c[t] += fwd[t, i]; // scaling coefficient
                }

                if (c[t] != 0) // Scaling
                {
                    for (int i = 0; i < States; i++)
                        fwd[t, i] = fwd[t, i] / c[t];
                }
            }

            return fwd;
        }

        /// <summary>
        ///   Baum-Welch backward pass (with scaling)
        /// </summary>
        /// <remarks>
        ///   Reference: http://courses.media.mit.edu/2010fall/mas622j/ProblemSets/ps4/tutorial.pdf
        /// </remarks>
        private double[,] backward(int[] observations, double[] c) {
            int T = observations.Length;
            double[] pi = Probabilities;
            double[,] A = Transitions;

            double[,] bwd = new double[T, States];

            // For backward variables, we use the same scale factors
            //   for each time t as were used for forward variables.

            // 1. Initialization
            for (int i = 0; i < States; i++)
                bwd[T - 1, i] = 1.0 / c[T - 1];

            // 2. Induction
            for (int t = T - 2; t >= 0; t--) {
                for (int i = 0; i < States; i++) {
                    double sum = 0;
                    for (int j = 0; j < States; j++)
                        sum += A[i, j] * B[j, observations[t + 1]] * bwd[t + 1, j];
                    bwd[t, i] += sum / c[t];
                }
            }

            return bwd;
        }

        /// <summary>
        ///   Checks if a model has converged given the likelihoods between two iterations
        ///   of the Baum-Welch algorithm and a criteria for convergence.
        /// </summary>
        private static bool checkConvergence(double oldLikelihood, double newLikelihood,
                int currentIteration, int maxIterations, double tolerance) {
            // Update and verify stop criteria
            if (tolerance > 0) {
                // Stopping criteria is likelihood convergence
                if (Math.Abs(oldLikelihood - newLikelihood) <= tolerance)
                    return true;

                if (maxIterations > 0) {
                    // Maximum iterations should also be respected
                    if (currentIteration >= maxIterations)
                        return true;
                }
            }
            else {
                // Stopping criteria is number of iterations
                if (currentIteration == maxIterations)
                    return true;
            }

            // Check if we have reached an invalid state
            if (Double.IsNaN(newLikelihood) || Double.IsInfinity(newLikelihood)) {
                return true;
            }

            return false;
        }
        #endregion

    }
}
