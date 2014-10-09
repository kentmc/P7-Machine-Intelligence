from numpy import *
from decimal import *
from sys import *
from learner import Learner
from decimal import *
from sys import *
from utilities import *
from dataLoader import DataLoader
import time

list1 = [[1, 2], [3, 4], [5, 6]]

list2 = [2, 3]

#for x in xrange(0, len(list1), 2):
#	print list1[x]

dataloader = DataLoader()

train_data = dataloader.load_sequences_from_file("../data/" + "1" + ".pautomac" + ".test")

comps = collect_unique_symbol_compositions(train_data, 2)

print comps.index([1, 1])