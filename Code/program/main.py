from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import BaseLine3GramLearner
from Learners.baumWelchLearner import BaumWelchLearner
from Learners.kentLearner import KentLearner

"""
Tests different learners on a dataset
"""

data_loader = DataLoader()
train_data = data_loader.load_test_file("pautomac_1.train")
test_data = data_loader.load_test_file("pautomac_1.test")
solution_data = data_loader.load_solution_file("pautomac_1.solution")

#Test different learners
#learner1 = RandomLearner(train_data, test_data, solution_data)
#print "RandomLearner score: {}".format(learner1.evaluate())

#learner2 = BaseLine3GramLearner(train_data, test_data, solution_data)
#print "BaseLine3GramLearner score: {}".format(learner2.evaluate())

#learner3 = BaumWelchLearner(20, train_data, test_data, solution_data)
#print "BaumWelchLearner score: {}".format(learner3.evaluate())

learner4 = KentLearner(20, train_data, test_data, solution_data)
print "KentLearner score: {}".format(learner4.evaluate())