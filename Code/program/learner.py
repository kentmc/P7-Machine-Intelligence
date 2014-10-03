from utilities import calc_perplexity_mesaure

class Learner:
    def __init__(self, train_data):
        self.train_data = train_data
        
    def calc_sequence_probability(self, symbol_sequence):
        raise NotImplementedError('Subclasses must override function calc_sequence_probability()!')
    
    def evaluate(self, test_sequences, solution_probabilities):       
        if len(test_sequences) != len(solution_probabilities):
            raise Exception("Error evaluating. test_data contains {} sequences, but solution_data contains {} solutions."\
                            .format(len(test_sequences), len(solution_probabilities)))
        
        # calculate probabilities
        guessed_probabilities = [0] * len(test_sequences)
        for i in range(0, len(test_sequences)):
            guessed_probabilities[i] = self.calc_sequence_probability(test_sequences[i])
        
        # smooth to adjust for 0 probabilities
        map((lambda x: x if x > 0 else 0.00000000000001), guessed_probabilities)
        
        # normalize
        sum_guesses = sum(guessed_probabilities)
        if sum_guesses != 0.0:
            for i in range(len(test_sequences)):
                guessed_probabilities[i] = guessed_probabilities[i] / sum_guesses
                
        return calc_perplexity_mesaure(solution_probabilities, guessed_probabilities)