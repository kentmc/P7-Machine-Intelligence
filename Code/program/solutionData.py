#
class SolutionData:
    '''
    Represents a solution
    '''
    def __init__(self, filePath):
        file = open(filePath, "r")
        lines = file.readlines()
        
        # Discard first line, which is just meta data
        self.probabilities = [0]*(len(lines)-1)
        
        #Cast the values in the file to floating points and save as a new list
        for i in range(1, len(lines)):
            self.probabilities[i-1] = float(lines[i])