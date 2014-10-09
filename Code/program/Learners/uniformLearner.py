from learner import Learner
from decimal import Decimal

class UniformLearner(Learner):
    def train(self, train_data):
        pass
    
    def name(self):
        return "Uniform Learner"
    
    def calc_sequence_probability(self, symbol_sequence):
        # these probabilities will be normalised by the evaluate function of the class learner
        return Decimal(1)
