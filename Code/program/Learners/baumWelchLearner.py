from learner import Learner
from numpy.random.mtrand import dirichlet
from numpy import *
from sys import *
from dataLoader import DataLoader
import utilities
from utilities import count_unique_symbols

# Rewritten from implementation found at http://ai.cs.umbc.edu/icgi2012/challenge/Pautomac/code/pautomac_baumwelch.py
# to fit our interface
class BaumWelchLearner(Learner):
    
    def __init__(self, num_states, train_data, test_sequences, solution_probabilities):
        Learner.__init__(self, train_data)
        self.test_sequences = test_sequences
        self.solution_probabilities = solution_probabilities
        self.num_states = num_states
        self.learn(self.train_data)
        
    def name(self):
        return "Baum Welch Learner"
    
    def learn(self, train_data):
        self.num_symbols = count_unique_symbols(train_data)
        ll_bound = 10.0
        self.model = self.randommodel(self.num_states, self.num_symbols)
        prev = -1.0
        ll = -1.0
        while prev == -1.0 or ll - prev > ll_bound:
            prev = ll
            self.model = self.iterateEM(self.model, train_data)
            probs = self.computeprobabilities(self.model, train_data)
            ll = self.loglikelihood(probs)
    
    def calc_sequence_probability(self, symbol_sequence):
        DPdict = dict()
        prob = self.computeprobability(self.model, symbol_sequence, DPdict)
        return prob
    
    def number(self, num):
        return float(num)
    
    # Normalize the values in an array to sum to 1.0
    def normalize(self, arr):
        sumarr = self.number(sum(arr))
        if sumarr != 0.0:
            for i in range(len(arr)):
                arr[i] = arr[i] / sumarr
    
    # A probabilistic non-deterministic finite state automaton model (I,F,S,T)
    # I = array of initial state probabilities
    # F = array of final probabilities
    # S = matrix of symbols probabilities per state
    # T = 3D matrix of transition probabilities given a symbol-state pair
    
    # Creates an model filled with 0 probabilities
    
    
    def emptymodel(self, numstates, alphabet):
        I = array([self.number(0.0)] * numstates)
        F = array([self.number(0.0)] * numstates)
        S = []
        for i in range(numstates):
            newrow = array([self.number(0.0)] * alphabet)
            S.append(newrow)
    
        T = []
        for i in range(alphabet):
            T.append([])
            for j in range(numstates):
                newrow = array([self.number(0.0)] * numstates)
                T[i].append(newrow)
    
        return (I, F, S, T)
    
    # Creates a fully connected model with random probabilities
    
    
    def randommodel(self, numstates, alphabet):
        I = array(dirichlet([1] * numstates))
        F = array([0.0] * numstates)
        S = []
        # F is treated as an end of string symbol
        for i in range(numstates):
            probs = dirichlet([1] * (alphabet + 1))
            newrow = array(probs[0:alphabet])
            self.normalize(newrow)
            S.append(newrow)
            F[i] = probs[alphabet]
    
        T = []
        for i in range(alphabet):
            T.append([])
            for j in range(numstates):
                newrow = array(dirichlet([1] * numstates))
                T[i].append(newrow)
    
        return (I, F, S, T)
    
    # Computes string probabilities forwards using a dict for hashing (recursion)
    
    
    def computeprobabilityrecursion(self, (I, F, S, T), sequence, index, state, DPdict):
        # Probability = P(final)
        if index >= len(sequence):
            DPdict[tuple([state])] = F[state]
            return F[state]
    
        # Return the already hashed result
        if DPdict.has_key(tuple([state] + sequence[index:len(sequence)])):
            return DPdict[tuple([state] + sequence[index:len(sequence)])]
    
        # For every possible next state s:
        # Probability += P(symbol) * P(transition to s) * P(future)
        symb_prob = S[state][sequence[index]]
        final_prob = F[state]
        prob = self.number(0.0)
        for nextstate in range(len(T[sequence[index]][state])):
            if T[sequence[index]][state][nextstate] > 0.0:
                trans_prob = T[sequence[index]][state][nextstate]
                future_prob = self.computeprobabilityrecursion(
                    (I, F, S, T), sequence, index + 1, nextstate, DPdict)
                prob = prob + (self.number(1.0) - final_prob) * \
                    symb_prob * trans_prob * future_prob
    
        # Hash the result
        DPdict[tuple([state] + sequence[index:len(sequence)])] = prob
        return prob
    
    # Computes string probabilities forwards using a dict for hashing
    
    
    def computeprobability(self, (I, F, S, T), sequence, DPdict):
        result = self.number(0.0)
    
        for state in range(len(I)):
            if I[state] > 0.0:
                result = result + \
                    I[state] * \
                    self.computeprobabilityrecursion(
                        (I, F, S, T), sequence, 0, state, DPdict)
        return result
    
    # Computes all probabilities in a given list of examples
    
    def computeprobabilities(self, (I, F, S, T), sett):
        probs = []
        DPdict = dict()
        a = 0
        for sequence in sett:
            a+=1
            probs.append(self.computeprobability((I, F, S, T), sequence, DPdict))
        return probs
    
    # Computes string probabilities backwards using a dict for hashing (recursion)
    
    
    def computeprobabilityrecursionreverse(self, (I, F, S, T), sequence, index, state, DPdict):
        # Probability = P(initial)
        if index == 0:
            DPdict[tuple([state])] = I[state]
            return I[state]
    
        # Return the already hashed result
        if DPdict.has_key(tuple([state] + sequence[0:index])):
            return DPdict[tuple([state] + sequence[0:index])]
    
        # For every possible previous state s:
        # Probability += P(symbol) * P(transition from s) * P(past)
        prob = self.number(0.0)
        for prevstate in range(len(I)):
            if T[sequence[index - 1]][prevstate][state] > 0.0:
                final_prob = F[prevstate]
                symb_prob = S[prevstate][sequence[index - 1]]
                trans_prob = T[sequence[index - 1]][prevstate][state]
                past_prob = self.computeprobabilityrecursionreverse(
                    (I, F, S, T), sequence, index - 1, prevstate, DPdict)
    
                prob = prob + \
                    ((self.number(1.0) - final_prob)
                     * symb_prob * trans_prob * past_prob)
    
        # Hash the result
        DPdict[tuple([state] + sequence[0:index])] = prob
        return prob
    
    # Computes string probabilities backwards using a dict for hashing
    
    
    def computeprobabilityreverse(self, (I, F, S, T), sequence, DPdict):
        result = self.number(0.0)
    
        # For every final state f:
        # Probability += P(end in f) * P(past)
        for state in range(len(I)):
            result = result + F[state] * self.computeprobabilityrecursionreverse(
                (I, F, S, T), sequence, len(sequence), state, DPdict)
        return result
    
    # Computes all probabilities in a given list of examples
    
    
    def computeprobabilitiesreverse(self, (I, F, S, T), sett):
        probs = []
        DPdict = dict()
        for sequence in sett:
            probs.append(self.computeprobabilityreverse((I, F, S, T), sequence, DPdict))
        return probs
    
    
    def iterateEM(self, (I, F, S, T), sett):
        backward = dict()
        probs = []
        a = 0
        print "Baum welch score (still iterating): {}".format(self.evaluate(self.test_sequences, self.solution_probabilities))
        for sequence in sett:
            a+=1
            probs.append(self.computeprobability((I, F, S, T), sequence, backward))
        # backward = P(s|start(q))
    
        forward = dict()
        a = 0
        for sequence in sett:
            a+=1
            self.computeprobabilityreverse((I, F, S, T), sequence, forward)
        # forward = P(s,end(q))
    
        (Inew, Fnew, Snew, Tnew) = self.emptymodel(self.num_states, self.num_symbols)
    
        # P(I(q)|s) =  P(I(q),s)/P(s)
        # P(I(q)|s) =  P(I(q))*P(s|start(q))/P(s)
        for state in range(len(I)):
            for seq in range(len(sett)):
                sequence = sett[seq]
                prob = probs[seq]
                key = tuple([state] + sequence)
                if backward.has_key(key):
                    Inew[state] = Inew[state] + ((I[state] * backward[key]) / prob)
        self.normalize(Inew)
    
        # P(F(q)|s) =  P(F(q),s)/P(s)
        # P(F(q)|s) =  P(end(q),s)*P(F(q))/P(s)
        for state in range(len(I)):
            for seq in range(len(sett)):
                sequence = sett[seq]
                prob = probs[seq]
                key = tuple([state] + sequence)
                if forward.has_key(key):
                    Fnew[state] = Fnew[state] + ((F[state] * forward[key]) / prob)
    
        # P(S(q,a)|s) =  P(S(q,a),s)/P(s)
        # P(S(q,a)|s) =  P(end(q),S(q,a),tail(q))/P(s)
        # P(S(q,a)|s) =  P(end(q),head(s))*P(tail(s)|start(q))/P(s)
            Stotal = self.number(0.0)
            for seq in range(len(sett)):
                sequence = sett[seq]
                prob = probs[seq]
                for index in range(len(sequence)):
                    key = tuple([state] + sequence[0:index])
                    if forward.has_key(key):
                        key2 = tuple([state] + sequence[index:len(sequence)])
                        if backward.has_key(key2):
                            symprob = forward[key] * backward[key2]
                            Snew[state][sequence[index]] = Snew[state][
                                sequence[index]] + (symprob / prob)
    
            if Fnew[state] != 0.0:
                Fnew[state] = Fnew[state] / (Fnew[state] + sum(Snew[state]))
            self.normalize(Snew[state])
    
        for state in range(len(I)):
            for seq in range(len(sett)):
                sequence = sett[seq]
                prob = probs[seq]
                for index in range(len(sequence)):
                    key1 = tuple([state] + sequence[0:index])
                    if forward.has_key(key1):
                        for state2 in range(len(I)):
                            key2 = tuple(
                                [state2] + sequence[(index + 1):len(sequence)])
                            if backward.has_key(key2):
                                transprob = (self.number(
                                    1.0) - F[state]) * S[state][sequence[index]] * T[sequence[index]][state][state2]
                                transprob = forward[key1] * \
                                    transprob * backward[key2]
                                Tnew[sequence[index]][state][state2] = Tnew[
                                    sequence[index]][state][state2] + (transprob / prob)
    
        for a in range(self.num_symbols):
            for state in range(len(I)):
                self.normalize(Tnew[a][state])
    
        return (Inew, Fnew, Snew, Tnew)
    
    
    def loglikelihood(self, probs):
        sumt = self.number(0.0)
        log2 = log10(self.number(2.0))
        for index in range(len(probs)):
            term = log10(probs[index]) / log2
            sumt = sumt + term
        return sumt