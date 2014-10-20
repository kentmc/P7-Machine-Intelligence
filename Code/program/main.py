from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import Baseline3GramLearner
from Learners.baumWelchLearner import BaumWelchLearner
from Learners.kentLearner import KentLearner
from Learners.baselineFrequencyLearner import BaselineFrequencyLearner
from Learners.theisLearner import TheisLearner
from Learners.uniformLearner import UniformLearner
from Learners.mathiasLearner import MathiasLearner
from Learners import baselineFrequencyLearner
from benchmarker import *

benchmarker = Benchmarker()
benchmarker.add_learners([MathiasLearner()])
#benchmarker.add_learners([UniformLearner(), KentLearner(), KentLearner(), TheisLearner(), Baseline3GramLearner(), BaselineFrequencyLearner()])

benchmarker.add_data_sets([10])
benchmarker.run_benchmark("results.txt")
print "Done"