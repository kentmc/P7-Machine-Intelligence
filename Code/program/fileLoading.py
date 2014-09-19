#Loads a solution file and returns a list, where each element i is the probability of generating the i'th sequence
def load_solution(self, filePath):
    file = open(filePath, "r")
    lines = file.readlines()
    # Discard first line, which is just meta data
    probabilities = [0]*(len(lines)-1)
    
    #Cast the values in the file to floating points and save as a new list
    for i in range(1, len(lines)):
        probabilities[i-1] = float(lines[i])