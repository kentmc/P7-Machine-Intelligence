class DataLoader:
    '''
    Class for loading test and solution files
    '''
    def __init__(self):
        self.directory = "../data/"

    def load_probabilities_from_file(self, file_name):
        """
        Loads a solution file and returns a list where each element i is the probability of generating the i'th sequence
        """
        file = open(self.directory+file_name, "r")
        lines = file.readlines()
        # Discard first line, which is just meta data
        probabilities = [0]*(len(lines)-1)
        
        #Cast the values in the file to floating points and save as a new list
        for i in range(1, len(lines)):
            probabilities[i-1] = float(lines[i])
        return probabilities
    
    def load_sequences_from_file(self, file_name):
        """
        Loads a test data file and returns the sequences as a list
        """
        f = open(self.directory+file_name)

        data = f.readline()
        length = map(int, data.split(' '))[0]
        sequences = []

        for num in xrange(0, length):
            temp_line = f.readline()
            temp_line = map(int, temp_line.split(' '))
            temp_line.pop(0)
            sequences.append(temp_line)
        f.close()

        return sequences