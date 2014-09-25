from learner import Learner
from Models.hmm import HMM

class RandomLearner(Learner):

    def learn(self, train_data):
        self.hmm = HMM()

    def calc_sequence_probability(self, symbol_sequence):
        
