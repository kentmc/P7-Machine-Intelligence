from learner import Learner
from decimal import *
from sys import *

class BaseLine3GramLearner(Learner):

    def __init__(self, train_data, test_data, solution_data):
        Learner.__init__(self, train_data, test_data, solution_data)
        self.threegramprobs = self.threegramdict(train_data)

    def calc_sequence_probability(self, symbol_sequence):
        prob = self.number('1.0')
        ngramseq = [-1, -1] + symbol_sequence + [-2]
        for start in range(len(ngramseq) - 2):
            end = start + 2
            prob = prob * (self.number(self.threegramprobs[tuple(ngramseq[start:end])][ngramseq[end]]) / self.number(self.threegramprobs[tuple(ngramseq[start:end])][-1]))
        return prob

    def number(self, arg):
        return Decimal(arg)

    def threegramdict(self, sett):
        DPdict = dict()
        total = 0
        for sequence in sett:
            ngramseq = [-1, -1] + sequence + [-2]
            for start in range(len(ngramseq) - 2):
                total
                end = start + 2
                if DPdict.has_key(tuple(ngramseq[start:end])):
                    table = DPdict[tuple(ngramseq[start:end])]
                    if table.has_key(ngramseq[end]):
                        table[ngramseq[end]] = table[ngramseq[end]] + 1
                    else:
                        table[ngramseq[end]] = 1
                    table[-1] = table[-1] + 1
                else:
                    table = dict()
                    table[ngramseq[end]] = 1
                    table[-1] = 1
                    DPdict[tuple(ngramseq[start:end])] = table
        return DPdict
