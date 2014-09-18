import random

class HMM:
    def __init__(self, numStates, numSymbols, randomize):
        self.numStates = numStates
        self.numSymbols = numSymbols
        # [a, b] = probability of a transition to state b being in state a
        self.transitionMatrix = [[0]*numStates for x in range(numStates)]
        # [a, b] = probability of state a emitting symbol b
        self.emissionMatrix = [[0]*numSymbols for x in range(numStates)]
        # [a] = probability of state a being the first state
        self.initialMatrix = [0]*numStates
        # [a] = probability of stopping when entering state a, without emitting a symbol
        self.stopMatrix = [0]*numStates
        
        if randomize: # Initialize all parameters at random
            # Initialize random parameters
            for x in range(0, numStates):
                self.initialMatrix[x] = random.uniform(0, 1.0)
                for y in range(0, numStates):
                    self.transitionMatrix[x][y] = random.uniform(0, 1.0)
                for s in range(0, numSymbols):
                    self.emissionMatrix[x][s] = random.uniform(0, 1.0)
            # Normalize transition matrix
            for x in range(0, numStates):
                sumVal = sum(self.transitionMatrix[x])
                for y in range(0, numStates):
                    self.transitionMatrix[x][y] /= sumVal
            # Normalize emission matrix
            for x in range(0, numStates):
                sumVal = sum(self.emissionMatrix[x])
                for y in range(0, numSymbols):
                    self.emissionMatrix[x][y] /= sumVal
            # Normalize initial matrix
            sumVal = sum(self.initialMatrix)
            for x in range(0, numStates):
                self.initialMatrix[x] /= sumVal
            # Stop matrix should not be normalized!


