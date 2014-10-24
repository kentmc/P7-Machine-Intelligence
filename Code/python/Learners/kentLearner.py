from learner import Learner
from Models.hmm import HMM
from numpy import random
from copy import deepcopy
from decimal import Decimal
import math
import utilities

class KentLearner(Learner):

    def train(self, train_data, test_data):
        self.train_data = train_data
        self.num_symbols = utilities.count_unique_symbols(train_data)
        self.i = 0
    
    def name(self):
        return "Kent Learner"
    
    def count_empty_sequences(self):
        return self.train_data.count([])
    
    def calc_sequence_probability(self, symbol_sequence):
        print str(self.i)

        if (len(symbol_sequence) == 0):
            return Decimal(self.count_empty_sequences()) / Decimal(len(self.train_data))
        
        prob = Decimal(1)
        start = 0
        end = 1
        
        while (end < len(symbol_sequence)):
            # search for largest sequence that is contained in training data
            seq = symbol_sequence[start:end]
            while self.is_member_of_train_data(seq):
                end = end + 1
                if (end > len(symbol_sequence)):
                    break
                seq = symbol_sequence[start:end]
            
            count, shingles_count = self.count_occurences(symbol_sequence[start:end-1])
            length = end - start - 1
            prob *= Decimal(count) * Decimal(math.pow(2, length)) / Decimal(shingles_count)
            start = end
        print str(self.i) + ": " +str(prob)
        self.i += 1
        return prob
    
    def is_member_of_train_data(self, seq):
        # returns whether the sequence seq is part of any sequence in the training data
        for train_seq in self.train_data:
            start = 0
            end = len(seq)
            while (end <= len(train_seq)):
                if seq == train_seq[start:end]:
                    return True
                start += 1
                end += 1
        return False
    
    def count_occurences(self, seq):
        # returns (count, shingle_count) where count is the  number of time seq occurs in training data
        # and shingle_count is the number of n-gram windows in the training data of size len(seq),
        # roughly the sum of length of all sequences / len(seq)
        shingle_count = 0
        count = 0
        for train_seq in self.train_data:
            start = 0
            end = len(seq)
            while (end <= len(train_seq)):
                shingle_count += 1
                if seq == train_seq[start:end]:
                    count += 1
                start += 1
                end += 1
                
        return count, shingle_count
                
                