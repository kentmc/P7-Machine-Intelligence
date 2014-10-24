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

def collect_unique_symbol_compositions(list_of_sequences, from_comp_length, to_comp_length):
    """
    Counts the number of unique symbols found in a list, assuming that all symbols are integers: 0, 1, ..., n
    """
    duplicates, unique = 0, 0
    composition_list = []
    for line in xrange(0, len(list_of_sequences)): #for every line 
        #print list_of_sequences[line]
        #print len(list_of_sequences[line])
        if len(list_of_sequences[line]) == 1:
            composition_list += list_of_sequences[line]

        elif 1 < len(list_of_sequences[line]) < (from_comp_length + to_comp_length):
            for symbol in xrange(0, len(list_of_sequences[line])): #for every symbol
                composition = [(list_of_sequences[line][symbol])]
                if composition in composition_list:
                    duplicates += 1
                else:
                    unique += 1
                    #print composition
                    #time.sleep(1)
                    composition_list += [composition]

        else:
            for symbol in xrange(0, len(list_of_sequences[line]) - (from_comp_length - 1) ): #for every symbol
                #from composition
                composition = get_comp_from_list(list_of_sequences[line], symbol, from_comp_length)
                if composition in composition_list:
                    duplicates += 1
                else:
                    unique += 1
                    composition_list += [composition]
                    #print "from" + str([composition])
                    #time.sleep(1)
            
            for symbol in xrange(0, len(list_of_sequences[line]) - (to_comp_length - 1) ):    
                #to composition
                composition = get_comp_from_list(list_of_sequences[line], symbol, to_comp_length)
                if composition in composition_list:
                    duplicates += 1
                
                else:
                    unique += 1
                    composition_list += [composition]
                    #print "to" + str([composition])
                    #time.sleep(1)
        
    print duplicates, unique
    #print composition_list
    return composition_list

def get_comp_from_list(comp_list, start_index, end_index):
    comp = []
    #print comp_list
    #print start_index, end_index
    for symbol in range(start_index, start_index + end_index):
        comp += [comp_list[symbol]]
    return comp
    
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
    
    
    