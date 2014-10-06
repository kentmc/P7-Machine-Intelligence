"""
WORK IN PROGRESS by Kent!
"""

from learner import Learner
from Models.hmm import HMM
from utilities import count_unique_symbols
from numpy import random
from decimal import *
from copy import deepcopy

class GreedyLearner(Learner):
    def __init__(self, num_states, train_data, test_sequences, solution_probabilities):
        Learner.__init__(self, train_data)
        self.test_sequences = test_sequences
        self.solution_probabilities = solution_probabilities
        self.num_states = num_states
        self.learn()
        
    
        
    def learn(self):
        self.num_symbols = count_unique_symbols(self.train_data)
        self.hmm = HMM(self.num_states, self.num_symbols)
        self.hmm.randomize()
        
        print "calc initial prob"
        probabilities = map(self.hmm.calc_sequence_probability, self.train_data)
        print "calc initial loglikelihood"
        loglikelihood = self.hmm.loglikelihood(probabilities)
        print "initial loglikelihood: {}".format(loglikelihood)
        # Add the stop symbol to the end of every sequence
        for i in range(len(self.train_data)):
            self.train_data[i].append(self.num_symbols)
            
        temperature = 1.0
        while temperature > 0.1:
            backup_hmm = self.hmm.clone()
            self.randomly_change_hmm_parameters(temperature)
            self.hmm.normalize()
            probabilities = map(self.hmm.calc_sequence_probability, self.train_data)
            new_loglikelihood = self.hmm.loglikelihood(probabilities)
            print "new loglikelihood: {}".format(new_loglikelihood)
            if new_loglikelihood > loglikelihood:
                loglikelihood = new_loglikelihood
            else:
                self.hmm = backup_hmm
            temperature = temperature * 0.9
            print "Likelihood: {}, temperature: {}".format(loglikelihood, temperature)
    
    def randomly_change_hmm_parameters(self, temperature):
        # Change transition matrix
        for x in range(0, self.num_states):
            for y in range(0, self.num_states):
                if random.uniform(0, temperature) < temperature:
                    self.hmm.transition_matrix[x][y] = Decimal(random.uniform(0, 1.0))
                
        # Change emission matrix
        for x in range(0, self.num_states):
            for y in range(0, self.num_symbols + 1):
                if random.uniform(0, temperature) < temperature:
                    self.hmm.emission_matrix[x][y] = Decimal(random.uniform(0, 1.0))

        # Change initial matrix
        for x in range(0, self.num_states):
            if random.uniform(0, temperature) < temperature:
                self.hmm.initial_matrix[x] = Decimal(random.uniform(0, 1.0))
           
    def calc_sequence_probability(self, symbol_sequence):
        return self.hmm.calc_sequence_probability(symbol_sequence)