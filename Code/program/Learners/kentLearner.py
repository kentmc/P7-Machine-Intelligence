from learner import Learner
from Models.hmm import HMM
from utilities import count_unique_symbols
from numpy import random
from copy import deepcopy

class KentLearner(Learner):

    def train(self, train_data):
        self.train_data = train_data
    
    def name(self):
        return "Kent Learner"
    
    def calc_sequence_probability(self, symbol_sequence):
        start = 0
        end = 1
        prob = 1
        last_count = 0
        last_shingle_count = 0

        # search for lowest sequence
        while end <= len(symbol_sequence):
            seq = symbol_sequence[start:end]
            count, shingle_count = self.count_occurences(seq)
            if count == 0:
                prob *= last_count / last_shingle_count
                start += len(last_shingle_count)
            last_count = count
            last_shingle_count = shingle_count
        return prob
    
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
        return count, shingle_count
                
                