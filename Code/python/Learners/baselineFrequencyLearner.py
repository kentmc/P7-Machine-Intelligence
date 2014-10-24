from learner import Learner
from decimal import Decimal

class BaselineFrequencyLearner(Learner):

    def train(self, train_data, test_data):
        sequences = train_data + test_data
        self.train_data_len = len(sequences)
        self.DPdict = dict()
        for sequence in sequences:
            if self.DPdict.has_key(tuple(sequence)):
                self.DPdict[tuple(sequence)] = self.DPdict[tuple(sequence)] + 1
            else:
                self.DPdict[tuple(sequence)] = 1
    
    def name(self):
        return "Baseline Frequency Learner"
    
    def calc_sequence_probability(self, symbol_sequence):
        return Decimal(self.DPdict[tuple(symbol_sequence)]) / Decimal(self.train_data_len)