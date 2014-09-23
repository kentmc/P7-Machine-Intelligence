from dataLoader import DataLoader
from Learners.randomLearner import RandomLearner
from Learners.baseline3gramLearner import BaseLine3GramLearner

data_loader = DataLoader()
train_data = data_loader.load_test_file("pautomac_1.train", "int")
test_data = data_loader.load_test_file("pautomac_1.test", "int")
solution_data = data_loader.load_solution_file("pautomac_1.solution")

#Test different learners
learner1 = RandomLearner()
print "Learner 1 score: {}".format(learner1.evaluate(test_data, solution_data))

learner2 = BaseLine3GramLearner()
learner2.learn(train_data)
print "Learner 2 score: {}".format(learner2.evaluate(test_data, solution_data))
