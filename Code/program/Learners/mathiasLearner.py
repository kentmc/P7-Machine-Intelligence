from numpy import *
from decimal import *
from sys import *
from learner import Learner
from decimal import *
from sys import *
from utilities import *

class MathiasLearner(Learner):
    """
    Counts the probability for one or more symbols to succeed 2 or more symbols
    """
    
    def name(self):
        return "Mathias Learner"
    
    def train(self, train_data):
        # number of symbols excluding the stop symbol
        num_symbols = count_unique_symbols(train_data)
        
        longest_sequence_len = longest_sequence_length(train_data)

        # not all possible combinations exist in the data, here we find which one does.        
        compositions = collect_unique_symbol_compositions(train_data, 2)

        # keeps track of occurences of symbols: [symbol_composition][symbol_composition]
        # note that this table can be very, very large: 
        # n = number of symbols - r = size of compositions 
        # n! / (n - r)!(r!) for instance: 23! / (23 - 4)!(4!) = 1771
        # 1771 * 1771 = 3.136.441 unique probabilities
        self.table = [[0]*(compositions) for x in range(compositions)]

        # using the composition list.index as our table index, we increment counts for transitions
        for line in range(len(train_data)):
            for symbol in range(len(train_data[line]))
                











        # increment counters for all compositions using all sequences in the test data
        for x in range(len(train_data)):                # for every line
            for y in range(len(train_data[x])):  # for every 2 symbol of that line
                if train_data
                self.table[y][train_data[x][y]] += 1

        #normalize
        for x in range(len(self.table)):
            sumval = sum(self.table[x])
            for y in range(len(self.table[x])):
                if sumval != 0:
                    self.table[x][y] = Decimal(self.table[x][y]) / Decimal(sumval)
                else:
                    self.table[x][y] = Decimal(1) / Decimal(compositions)
    
    def calc_sequence_probability(self, symbol_sequence):
        prob = Decimal(1)
        for x in xrange(0, len(symbol_sequence), 2):
            prob *= self.table[x][symbol_sequence[x]]
        return prob