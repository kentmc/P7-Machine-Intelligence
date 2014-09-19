import math
class Model:
    # The probability that the given model have generated the specified sequence of symbols
    def generating_probability(self, symbol_sequence):
        raise NotImplementedError('Subclasses must override generating_probability()!')
    
    # Evaluates the score of the model based on the Pautomac evaluation criteria
    def evaluate(self, dataset, solution_data):
        if len(dataset) != len(solution_data.probabilities):
            print "Error evaluation HMM. DataSet contains " + len(dataset.sequences) + " sequences, "\
            + "but solution_data contains " + len(solution_data.probabilities) + " solutions."
            return 0
        score = 0
        for i in range(0, dataset.sequences):
            guessed_pr = self.generating_probability(dataset.sequences[i])
            real_pr = solution_data.probabilities[i]
            score += math.pow(2, realPr * math.log(guessed_pr))
        return score