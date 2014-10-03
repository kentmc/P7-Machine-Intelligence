class Model:
    # Returns the probability of generating the specified sequence of symbols
    def calc_sequence_probability(self, symbol_sequence):
        raise NotImplementedError('Subclasses must override function calc_sequence_probability()!')