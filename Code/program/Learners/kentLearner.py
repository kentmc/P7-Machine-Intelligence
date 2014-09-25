from learner import Learner
from Models.hmm import HMM
from utilities import count_unique_symbols
from numpy import random

class KentLearner(Learner):

    def __init__(self, num_states, test_data, solution_data):
        self.solution_data = solution_data
        self.test_data = test_data
        self.num_states = num_states
        
    def learn(self, train_data):
        self.num_symbols = count_unique_symbols(train_data)
        self.hmm = HMM(self.num_states, self.num_symbols)
        
        # Add the stop symbol to the end of every sequence
        for i in range(len(train_data)):
            train_data[i].append(self.num_symbols)
            
        # Guess randomly which states generated every sequence
        state_sequences = []
        for i in range(len(train_data)):
            lst = []
            for p in range(len(train_data[i])):
                guessed_state = random.randint(self.num_states - 1)
                lst.append(guessed_state)
            state_sequences.append(lst)
            
        # calculate parameters of hmm based on the guessed state sequences
        self.hmm = self.estimate_hmm(state_sequences, train_data)    
        
        for i in range(10):
            print "Score after {} iterations: {}".format(i, self.evaluate(self.test_data, self.solution_data))
            self.iterate(train_data)
    
    def iterate(self, train_data):
        # given new parameters, calculate the most probable sequence of states to yield each sequence
        state_sequences = []
        for i in range(len(train_data)):
            prob, state_seq = self.hmm.viterbi(train_data[i])
            state_sequences.append(state_seq)
        
        # calculate the hmm parameters from the estimated states
        self.hmm = self.estimate_hmm(state_sequences, train_data)

    def estimate_hmm(self, state_sequences, train_data):
        estimated_hmm = HMM(self.num_states, self.num_symbols)
        
        for i in range(len(state_sequences)):
            estimated_hmm.initial_matrix[state_sequences[i][0]]+=1
            for p in xrange(1, len(state_sequences[i])):
                s_from = state_sequences[i][p-1]
                s_to = state_sequences[i][p]
                estimated_hmm.transition_matrix[s_from][s_to]+=1
                estimated_hmm.emission_matrix[s_from][train_data[i][p]]+=1
        estimated_hmm.normalize()
        return estimated_hmm
                
    def calc_sequence_probability(self, symbol_sequence):
        return self.hmm.calc_sequence_probability(symbol_sequence)
        
