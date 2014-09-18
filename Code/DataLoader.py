class DataLoader:
    '''
    Hej basse
    '''

    def __init__(self):
        pass

    def getData(fileName, DataType):

        f = open(fileName)

        data = f.readline()
        if DataType == 'int':
            length, states = map(int, data.split(' '))
        elif DataType == 'float':
            length, states = map(float, data.split(' '))

        data = []

        for num in xrange(0, length - 1):
            tempLine = f.readline()
            if DataType == 'int':
                tempLine = map(int, tempLine.split(' '))
            elif DataType == 'float':
                tempLine = map(float, tempLine.split(' '))
            tempLine.pop(0)
            data.append(tempLine)

        f.close()

        return data
