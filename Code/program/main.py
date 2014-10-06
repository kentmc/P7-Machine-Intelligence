from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import Baseline3GramLearner
from Learners.baumWelchLearner import BaumWelchLearner
from Learners.kentLearner import KentLearner
from Learners.greedyLearner import GreedyLearner
from Learners.baselineFrequencyLearner import BaselineFrequencyLearner
from Learners.theisLearner import TheisLearner
from Models.hmm import HMM
from Learners import baselineFrequencyLearner
from benchmarker import *

benchmarker = Benchmarker()
benchmarker.add_learners([RandomLearner(), TheisLearner()])
benchmarker.add_data_sets([1, 2, 3, 4, 5])
benchmarker.run_benchmark("C:/Users/Kent/Desktop/testresult.txt")
print "Done"