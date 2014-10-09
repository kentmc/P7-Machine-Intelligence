from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import Baseline3GramLearner
from Learners.baumWelchLearner import BaumWelchLearner
from Learners.kentLearner import KentLearner
from Learners.greedyLearner import GreedyLearner
from Learners.baselineFrequencyLearner import BaselineFrequencyLearner
from Learners.theisLearner import TheisLearner
from Learners.uniformLearner import UniformLearner
from Learners import baselineFrequencyLearner
from benchmarker import *

benchmarker = Benchmarker()
#benchmarker.add_learners([UniformLearner(), BaumWelchLearner(20), TheisLearner(), Baseline3GramLearner(), BaselineFrequencyLearner()])
benchmarker.add_learners([UniformLearner(), KentLearner(), Baseline3GramLearner(), BaselineFrequencyLearner()])

benchmarker.add_data_sets([1])
benchmarker.run_benchmark("C:/Users/Kent/Desktop/testresult2.txt")
print "Done"