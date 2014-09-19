import unittest
from hmm import HMM

class TestHMM(unittest.TestCase):
    def test_hmm_small_simple(self):
        #A HMM with all parameters set to zero
        hmm = HMM(2, 2, False)
        hmm.emission_matrix = [[0.5, 0.5], [0.5, 0.5]]
        hmm.initial_matrix = [0.5, 0.5]
        hmm.transition_matrix = [[0.5, 0.5], [0.5, 0.5]]
        self.assertEqual(hmm.generating_probability([0]), 0.5)
        
    def test_hmm_small_moderate(self):
        #A HMM with all parameters set to zero
        hmm = HMM(2, 2, False)
        hmm.emission_matrix = [[0.1, 0.9], [0, 1]]
        hmm.initial_matrix = [0.1, 0.9]
        hmm.transition_matrix = [[0.1, 0.9], [0, 1]]
        self.assertAlmostEqual(hmm.generating_probability([0, 0, 0]), 0.000001, 10)