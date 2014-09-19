import unittest
from hmm import HMM

class TestHMM(unittest.TestCase):
    def test_hmm_simple(self):
        #A HMM with all parameters set to zero
        hmm = HMM(2, 2, False)
        hmm.emission_matrix = [0.5, 0.5]
        hmm.initial_matrix = [0.5, 0.5]
        hmm.transition_matrix = [[0.5, 0.5], [0.5, 0.5]]
        self.assertEqual(hmm.generating_probability([0]), 0.5)