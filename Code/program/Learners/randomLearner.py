from learner import Learner
import random

class RandomLearner(Learner):
    def train(self, train_data):
        pass
    
    def name(self):
        return "Random Learner"
    
    def calc_sequence_probability(self, symbol_sequence):
        return random.uniform(0.0, 1.0)
