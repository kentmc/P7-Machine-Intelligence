def count_unique_symbols(list_of_sequences):
    """
    Counts the number of unique symbols found in a list, assuming that all symbols are integers: 0, 1, ..., n
    """
    max_val_found = 0
    for i in xrange(0, len(list_of_sequences)):
        for p in xrange(0, len(list_of_sequences[i])):
            if (list_of_sequences[i][p] > max_val_found):
                max_val_found = list_of_sequences[i][p]
                
    # Add 1 because first symbol is 0
    return max_val_found + 1