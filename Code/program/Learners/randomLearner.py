from learner import Learner
import random

class RandomLearner(Learner):
    def __init__(self, train_data):
        Learner.__init__(self, train_data)
        
    def calc_sequence_probability(self, symbol_sequence):
        return random.uniform(0.0, 1.0)
