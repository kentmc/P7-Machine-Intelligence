from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import BaseLine3GramLearner
from Learners.baumWelchLearner import BaumWelchLearner
from Learners.kentLearner import KentLearner
from Learners.greedyLearner import GreedyLearner
from Models.hmm import HMM

"""
Tests different learners on a dataset
"""

data_loader = DataLoader()
train_data = data_loader.load_sequences_from_file("pautomac_1.train")
test_data = data_loader.load_sequences_from_file("pautomac_1.test")
train_data = train_data + test_data
solution_data = data_loader.load_probabilities_from_file("pautomac_1.solution")

#Test different learners
learner1 = RandomLearner(train_data)
print "RandomLearner score: {}".format(learner1.evaluate(test_data, solution_data))
learner2 = BaseLine3GramLearner(train_data)
print "BaseLine3GramLearner score: {}".format(learner2.evaluate(test_data, solution_data))
#learner3 = BaumWelchLearner(20, train_data, test_data, solution_data)
#print "BaumWelchLearner score: {}".format(learner3.evaluate(test_data, solution_data))
learner4 = KentLearner(20, train_data, test_data, solution_data)
print "KentLearner score: {}".format(learner4.evaluate(test_data, solution_data))