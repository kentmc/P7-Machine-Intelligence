from learner import Learner
from Models.hmm import HMM
from utilities import count_unique_symbols
from numpy import random
from copy import deepcopy

class KentLearner(Learner):

    def train(self, num_states, train_data):
        self.num_states = num_states
        self.learn()
    
    def name(self):
        return "Kent Learner"
    
    def learn(self):
        self.num_symbols = count_unique_symbols(self.train_data)
        self.hmm = HMM(self.num_states, self.num_symbols)
        
        # Add the stop symbol to the end of every sequence
        for i in range(len(self.train_data)):
            self.train_data[i].append(self.num_symbols)
            
        # Guess randomly which states generated every sequence
        state_sequences = []
        for i in range(len(self.train_data)):
            lst = []
            for p in range(len(self.train_data[i])):
                guessed_state = random.randint(self.num_states - 1)
                lst.append(guessed_state)
            state_sequences.append(lst)
            
        # calculate parameters of hmm based on the guessed state sequences
        self.hmm = self.estimate_hmm(state_sequences, self.train_data)    
        
        for i in range(5000):
            probabilities = map(self.hmm.calc_sequence_probability, self.train_data)
            print "Loglikelihood after {} iterations: {}".format(i, self.hmm.loglikelihood(probabilities))
            self.iterate()
    
    def iterate(self):
        # given new parameters, calculate the most probable sequence of states to yield each sequence
        state_sequences = []
        for i in range(len(self.train_data)):
            prob, state_seq = self.hmm.viterbi(self.train_data[i])
            state_sequences.append(state_seq)
        
        # calculate the hmm parameters from the estimated states
        estimated_hmm = self.estimate_hmm(state_sequences, self.train_data)
        
        self.hmm = estimated_hmm
        #self.greedy_replace_rows(self.hmm, estimated_hmm)
        

    def greedy_replace_rows(self, hmm, hmm_other):
        """
        Replaces any row in hmm's transition, emission or initial matrix, it it improves its score
        """

        initial_score = self.evaluate()
        current_score = -1
        while current_score != initial_score: # continue as long as improvements happen
            print "score: {}".format(self.evaluate())
            current_score = initial_score
            
            # initial matrix
            backup_row = deepcopy(hmm.initial_matrix)
            hmm.initial_matrix = hmm_other.initial_matrix
            new_score = self.evaluate()
            if new_score > current_score:
                hmm.initial_matrix = backup_row
            else:
                current_score = new_score
            
            # transition matrix
            for i in range(hmm.num_states):
                backup_row = deepcopy(hmm.transition_matrix[i])
                hmm.transition_matrix[i] = hmm_other.transition_matrix[i]
                new_score = self.evaluate()
                if new_score > current_score:
                    hmm.transition_matrix[i] = backup_row
                else:
                    current_score = new_score
                    
            # emission matrix
            for i in range(hmm.num_states):
                backup_row = deepcopy(hmm.emission_matrix[i])
                hmm.emission_matrix[i] = hmm_other.emission_matrix[i]
                new_score = self.evaluate()
                if new_score > current_score:
                    hmm.emission_matrix[i] = backup_row
                else:
                    current_score = new_score
                

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
