def count_unique_symbols(test_data):
    """
    Counts the number of unique symbols found in test data
    """
    max_val_found = 0
    for i in xrange(0, len(test_data)):
        for p in xrange(0, len(test_data[i])):
            if (test_data[i][p] > max_val_found):
                max_val_found = test_data[i][p]
                
    # Add 1 because first symbol is 0
    return max_val_found + 1