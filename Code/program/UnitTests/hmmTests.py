import unittest
from hmm import HMM

class TestHMM(unittest.TestCase):
    def test_hmm_3_2_1(self):
        hmm = HMM(3, 2, False)
        hmm.emission_matrix = [[0.5, 0.3, 0.2], [0, 1, 0], [0.1, 0.1, 0.8]]
        hmm.initial_matrix = [0.1, 0.2, 0.7]
        hmm.transition_matrix = [[0, 1, 0], [0.85, 0.1, 0.05], [0.3, 0.2, 0.5]]
        self.assertEqual(hmm.calc_probability([0]), 0.0322)
