import math

class Learner:
    def learn(self, train_data):
        raise NotImplementedError('Subclasses must override function learn()!')
    
    def calc_sequence_probability(self, symbol_sequence):
        raise NotImplementedError('Subclasses must override function calc_sequence_probability()!')
    
    def evaluate(self, test_data, solution_data):       
        if len(test_data) != len(solution_data):
            raise Exception("Error evaluating. test_data contains {} sequences, but solution_data contains {} solutions."\
                            .format(len(test_data), len(solution_data)))
        
        # calculate probabilities
        guessed_probabilities = [0] * len(test_data)
        for i in range(0, len(test_data)):
            guessed_probabilities[i] = self.calc_sequence_probability(test_data[i])
        
        # normalize
        sum_guesses = sum(guessed_probabilities)
        if sum_guesses != 0.0:
            for i in range(len(test_data)):
                guessed_probabilities[i] = guessed_probabilities[i] / sum_guesses
                
        # calculate score
        score = 0
        for i in range(0, len(test_data)):
            real_pr = solution_data[i]
            guessed_pr = guessed_probabilities[i]
            if guessed_pr != 0:
                score += math.pow(2, real_pr * math.log(guessed_pr, 2))
        return score