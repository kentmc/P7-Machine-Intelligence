from decimal import *
import math

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

def longest_sequence_length(list_of_sequences):
    """
    Returns the length of the longest sequence
    """
    maxlength = 0
    for seq in list_of_sequences:
        if len(seq) > maxlength:
            maxlength = len(seq)
    return maxlength

def calc_perplexity_mesaure(real_probabilities, guessed_probabilities):
    # calculate score
        score = Decimal(0)
        for i in range(0, len(guessed_probabilities)):
            real_pr = real_probabilities[i]
            guessed_pr = guessed_probabilities[i]
            score += Decimal(real_pr) * Decimal(math.log(guessed_pr, 2))
        return math.pow(2, -score)
    
    
    