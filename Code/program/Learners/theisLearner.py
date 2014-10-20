from utilities import count_unique_symbols, longest_sequence_length
from learner import Learner
from decimal import Decimal

class TheisLearner(Learner):
    """
    Counts the probability for each symbol occuring at each position in a sequence
    """
    
    def name(self):
        return "Theis Learner"
    
    def train(self, train_data, test_data):
        sequences = train_data + test_data
        
        # number of symbols excluding the stop symbol
        num_symbols = count_unique_symbols(sequences)
        
        longest_sequence_len = longest_sequence_length(sequences)
        
        # keeps track of occurences of symbols at different positions: [position][symbol]
        self.table = [[0]*(num_symbols) for x in range(longest_sequence_len)]
        
        # increment counters for all symbols at all position, using all sequences in the test data
        for x in range(len(sequences)):
            for y in range(len(sequences[x])):
                self.table[y][sequences[x][y]] += 1
            # increment the stop symbol count
            
        #normalize
        for x in range(len(self.table)):
            sumval = sum(self.table[x])
            for y in range(len(self.table[x])):
                if sumval != 0:
                    self.table[x][y] = Decimal(self.table[x][y]) / Decimal(sumval)
                else:
                    self.table[x][y] = Decimal(1) / Decimal(num_symbols)
    
    def calc_sequence_probability(self, symbol_sequence):
        prob = Decimal(1)
        for x in range(len(symbol_sequence)):
            prob *= self.table[x][symbol_sequence[x]]
        return prob