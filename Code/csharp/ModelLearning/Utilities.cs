using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelLearning {
    static class Utilities {
       /// <summary>
        /// Shuffle any (I)List with an extension method based on the Fisher-Yates shuffle :
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="list">List so shuffle</param>
       /// <param name="seed">Seed to use (default: -1 is system time specific seed)</param>
        public static void Shuffle<T>(IList<T> list, int seed = -1) {
            Random rng = seed == -1 ? new Random() : new Random(seed);
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static double Mean(this IEnumerable<double> list) {
            return list.Average(); // :-)
        }

        public static double Median(this IEnumerable<double> list) {
            List<double> orderedList = list
                .OrderBy(numbers => numbers)
                .ToList();

            int listSize = orderedList.Count;
            double result;

            if (listSize % 2 == 0) // even
            {
                int midIndex = listSize / 2;
                result = ((orderedList.ElementAt(midIndex - 1) +
                           orderedList.ElementAt(midIndex)) / 2);
            }
            else // odd
            {
                double element = (double)listSize / 2;
                element = Math.Round(element, MidpointRounding.AwayFromZero);

                result = orderedList.ElementAt((int)(element - 1));
            }

            return result;
        }
    }
}
