from numpy import *
from decimal import *
from sys import *
from learner import Learner
from decimal import *
from sys import *
from utilities import *
import time

class MathiasLearner(Learner):
    """
    Counts the probability for one or more symbols to succeed 2 or more symbols
    """
    
    def name(self):
        return "Mathias Learner"
    
    def train(self, train_data, test_data):
        # number of symbols excluding the stop symbol
        train_data += test_data
        num_symbols = count_unique_symbols(train_data)
        
        longest_sequence_len = longest_sequence_length(train_data)

        # not all possible combinations exist in the data, here we find which one does.        
        self.compositions = collect_unique_symbol_compositions(train_data, 2)
        print self.compositions
        # keeps track of occurences of symbols: [symbol_composition][symbol_composition]
        # note that this table can be very, very large: 
        # n = number of symbols - r = size of self.compositions 
        # n! / (n - r)!(r!) for instance: 23! / (23 - 4)!(4!) = 1771
        # 1771 * 1771 = 3.136.441 unique probabilities
        self.table = [[0]*(len(self.compositions)+1) for x in range(len(self.compositions)+1)]

        # using the composition list.index as our table index, we increment counts for transitions
        for line in range(len(train_data)):
            #print "line {}".format(line)
            for symbol in xrange(0, len(train_data[line]) - 3):
                from_comp = self.compositions.index([train_data[line][symbol]] + [train_data[line][symbol + 1]])   #[a, b]
                to_comp = self.compositions.index([train_data[line][symbol + 2]] + [train_data[line][symbol + 3]]) #[c, d]
                #print "from comp {} to comp {}".format([train_data[line][symbol]] + [train_data[line][symbol + 1]], [train_data[line][symbol + 2]] + [train_data[line][symbol + 3]])
                self.table[from_comp][to_comp] += 1 #note that our table cannot assign prob. for every possible transition. 
                                                    #Either train with test data, or expand the table if new unknown transitions are found.

        # # increment counters for all self.compositions using all sequences in the test data
        # for x in range(len(train_data)):                # for every line
        #     for y in range(len(train_data[x])):  # for every 2 symbol of that line
        #         if train_data
        #         self.table[y][train_data[x][y]] += 1

        #normalize
        for line in range(len(self.table)):
            sumval = sum(self.table[line])
            for symbol in range(len(self.table[line])):
                if sumval != 0:
                    self.table[line][symbol] = Decimal(self.table[line][symbol]) / Decimal(sumval)
                else:
                    self.table[line][symbol] = Decimal(1) / Decimal(len(self.compositions))
    
    def calc_sequence_probability(self, symbol_sequence):
        prob = Decimal(1)
        for symbol in xrange(0, len(symbol_sequence) - 3, 1):
            from_comp = self.compositions.index([symbol_sequence[symbol]] + [symbol_sequence[symbol + 1]])   #[a, b]
            to_comp = self.compositions.index([symbol_sequence[symbol + 2]] + [symbol_sequence[symbol + 3]]) #[c, d]
            prob *= self.table[from_comp][to_comp]
        return prob












