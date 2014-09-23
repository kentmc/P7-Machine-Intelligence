class DataLoader:
    '''
    Class for loading test and solution files
    '''
    def __init__(self):
        self.directory = "../data/"

    def load_solution_file(self, file_name):
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
    
    def load_test_file(self, file_name, data_type):
        """
        Loads a test data file and returns all symbol sequences in the following manner
        [[3, 2, 5, 3], [], [2, 9, 4], ...]
        """
        f = open(self.directory+file_name)

        data = f.readline()
        if data_type == 'int':
            length, states = map(int, data.split(' '))
        elif data_type == 'float':
            length, states = map(float, data.split(' '))

        data = []

        for num in xrange(0, length):
            temp_line = f.readline()
            if data_type == 'int':
                temp_line = map(int, temp_line.split(' '))
            elif data_type == 'float':
                temp_line = map(float, temp_line.split(' '))
            temp_line.pop(0)
            data.append(temp_line)

        f.close()

        return data
