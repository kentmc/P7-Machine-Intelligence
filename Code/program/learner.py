import math
from decimal import *

class Learner:
    def __init__(self, train_data, test_data, solution_data):
        self.train_data = train_data
        self.test_data = test_data
        self.solution_data = solution_data
        
    def calc_sequence_probability(self, symbol_sequence):
        raise NotImplementedError('Subclasses must override function calc_sequence_probability()!')
    
    def evaluate(self):       
        if len(self.test_data) != len(self.solution_data):
            raise Exception("Error evaluating. test_data contains {} sequences, but solution_data contains {} solutions."\
                            .format(len(self.test_data), len(self.solution_data)))
        
        # calculate probabilities
        guessed_probabilities = [0] * len(self.test_data)
        for i in range(0, len(self.test_data)):
            guessed_probabilities[i] = self.calc_sequence_probability(self.test_data[i])
        
        # normalize
        sum_guesses = sum(guessed_probabilities)
        if sum_guesses != 0.0:
            for i in range(len(self.test_data)):
                guessed_probabilities[i] = guessed_probabilities[i] / sum_guesses
                
        summ = sum(guessed_probabilities)
                
        # calculate score
        score = Decimal(0)
        for i in range(0, len(self.test_data)):
            real_pr = self.solution_data[i]
            guessed_pr = guessed_probabilities[i]
            if guessed_pr != 0:
                score += Decimal(real_pr) * Decimal(math.log(guessed_pr, 2))
        return math.pow(2, -score)