from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import BaseLine3GramLearner
from Learners.baumWelch import BaumWelchLearner
from Learners.kentLearner import KentLearner

"""
Tests different learners on a dataset
"""

data_loader = DataLoader()
train_data = data_loader.load_test_file("pautomac_1.train")
test_data = data_loader.load_test_file("pautomac_1.test")
solution_data = data_loader.load_solution_file("pautomac_1.solution")

#Test different learners
#learner1 = RandomLearner()
 # No need to learn anything because it makes a random guess anyway
#print "RandomLearner score: {}".format(learner1.evaluate(test_data, solution_data))

#learner2 = BaseLine3GramLearner()
#learner2.learn(train_data)
#print "BaseLine3GramLearner score: {}".format(learner2.evaluate(test_data, solution_data))

#learner3 = BaumWelchLearner(20)
#learner3.learn(train_data)
#print "BaumWelchLearner score: {}".format(learner3.evaluate(test_data, solution_data))

learner4 = KentLearner(20, test_data, solution_data)
learner4.learn(train_data)
#print "BaumWelchLearner score: {}".format(learner3.evaluate(test_data, solution_data))