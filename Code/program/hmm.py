import random
from model import Model

class HMM(Model):
    def __init__(self, num_states, num_symbols, randomize):
        """
        An extra symbol is added implicitly as a terminating symbol
        """
        self.num_states = num_states
        self.num_symbols = num_symbols
        # [a, b] = probability of a transition to state b being in state a
        self.transition_matrix = [[0]*(num_states) for x in range(num_states)]
        # [a, b] = probability of state a emmitting symbol b (the additional symbol is the stop symbol)
        self.emission_matrix = [[0]*(num_symbols + 1) for x in range(num_states)]
        # [a] = probability of state a being the first state
        self.initial_matrix = [0]*num_states
        
        if randomize: # Initialize all parameters at random
            # Initialize random parameters
            for x in range(0, num_states):
                self.initial_matrix[x] = random.uniform(0, 1.0)
                for y in range(0, num_states):
                    self.transition_matrix[x][y] = random.uniform(0, 1.0)
                for s in range(0, num_symbols + 1):
                    self.emission_matrix[x][s] = random.uniform(0, 1.0)
            # Normalize transition matrix
            for x in range(0, num_states):
                sumVal = sum(self.transition_matrix[x])
                for y in range(0, num_states):
                    self.transition_matrix[x][y] /= sumVal
            # Normalize emission matrix
            for x in range(0, num_states):
                sumVal = sum(self.emission_matrix[x])
                for y in range(0, num_symbols + 1):
                    self.emission_matrix[x][y] /= sumVal
            # Normalize initial matrix
            sumVal = sum(self.initialMatrix)
            for x in range(0, num_states):
                self.initial_matrix[x] /= sumVal
            # Stop matrix should not be normalized!    
    def calc_probability(self, symbol_sequence):
        """
        Calculates the probability of generating the specified symbol sequence
        """
        #[x, y] = probability of being in state x after generating the first y symbols of symbolSequence
        dynamic_array = [[0]*self.num_states for x in range(len(symbol_sequence)+1)]
        #construct base case
        for s in range(0, self.num_states):
            dynamic_array[0][s] = self.initial_matrix[s]
        #using dynamic programming
        for i in range(1, len(symbol_sequence)+1):
            for s in range(0, self.num_states):
                prob = 0
                for t in range(0, self.num_states):
                    prob += dynamic_array[i-1][t] * self.emission_matrix[t][symbol_sequence[i-1]] * self.transition_matrix[t][s]
                dynamic_array[i][s] = prob
        #sum over all possibilities of emitting the terminal symbol from all states after having generated the specified sequence
        summarize = 0
        for i in range(0, self.num_states):
            summarize += dynamic_array[len(symbol_sequence)][i] * self.emission_matrix[i][self.num_symbols]
        return summarize
            
                    
                    
        
        
        


