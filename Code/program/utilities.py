from decimal import Decimal
from math import log
from math import pow
import time

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

def collect_unique_symbol_compositions(list_of_sequences, composition_length = 2):
    """
    Counts the number of unique symbols found in a list, assuming that all symbols are integers: 0, 1, ..., n
    """
    duplicates, unique = 0, 0
    composition_list = []
    for line in xrange(0, len(list_of_sequences)): #for every line 
        for symbol in xrange(0, len(list_of_sequences[line]) - (composition_length - 1)): #for every symbol
            #print len(list_of_sequences[line]) - (composition_length - 1)
            composition = [(list_of_sequences[line][symbol])] + [(list_of_sequences[line][symbol+1])]
            #print composition
            #time.sleep(1)
            if composition in composition_list:
                duplicates += 1
            else:
                unique += 1
                #print "new comp {}".format(composition)  
                composition_list += [composition]
    
    print "{} duplicates {} unique compositions".format(duplicates, unique)             
    return composition_list

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
            score += Decimal(real_pr) * Decimal(log(guessed_pr, 2))
        return pow(2, -score)
    
    
    