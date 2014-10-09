from dataLoader import DataLoader

class Benchmarker:
    def __init__(self):
        self.learners = []
        self.dataset_ids = []
    def add_learners(self, list_of_learners):
        """
        Adds a number of learners to the benchmarker, by providing a list of instantiated learner objects,
        e.g.: [new baumWelcLearner(), new randomLearner()]
        """
        self.learners += list_of_learners
        
    def add_data_sets(self, list_of_data_set_ids):
        """
        Adds a number of datasets to the benchmarker, by providing their id from the Pautomac website,
        in the range [1:48], e.g. [1, 2, 39, 48]
        """
        self.dataset_ids += list_of_data_set_ids
        
    def run_benchmark(self, output_file_path):
        """
        Run benchmark for all learners on every dataset provided.
        The result will be output to the provided file.
        """
        dataloader = DataLoader()
        
        f = open(output_file_path,'w')
        
        # score, learner_1_name, learner_2_name, ..., learner_n_name
        f.write("dataset")
        for learner in self.learners:
            f.write(", " + learner.name())
        
        # write scores for all data sets
        for dataset_id in self.dataset_ids:
            print "Benchmarking dataset: " + str(dataset_id)
            f.write("\n" + str(dataset_id))
            train_data = dataloader.load_sequences_from_file("../data/" + str(dataset_id) + ".pautomac" + ".train")
            test_data = dataloader.load_sequences_from_file("../data/" + str(dataset_id) + ".pautomac" + ".test")
            solution_data = dataloader.load_probabilities_from_file("../data/" + str(dataset_id) + ".pautomac_solution" + ".txt")
            for learner in self.learners:
                print "Training learner: " + learner.name()
                learner.train(train_data+test_data)
                print "Evaluating learner: " + learner.name()
                score = learner.evaluate(test_data, solution_data)
                print "Achieved score: " + str(score)
                str_score = " {0:.1f}".format(score)
                while len(str_score) < 8:
                    str_score = " " + str_score
                f.write(", " + str_score)
        f.close()