using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLearning {
    static class DataLoader {
        public static SequenceData LoadSequences(string file) {
            string[] lines = System.IO.File.ReadAllLines(file);
            //parse number of different symbols
            int num_symbols = Int32.Parse(lines[0].Split(' ')[1]);
            SequenceData seqData = new SequenceData(num_symbols);

            //parse sequences
            for (int i = 1; i < lines.Length; i++) {
                string[] currentSeqStr = lines[i].Split(' ');

                //skip first element on each line, which contains the length of the sequence
                int[] currentSeq = currentSeqStr.Skip(1).Select(p => Int32.Parse(p)).ToArray();
                seqData.AddSequence(currentSeq);
            }
            seqData.SaveAddedSequences();
            return seqData;
        }

        public static double[] LoadSolutions(string file) {
            string[] lines = System.IO.File.ReadAllLines(file);
            //parse number of different symbols
            int numSequences = Int32.Parse(lines[0]);
            double[] solutions = new double[numSequences];

            //parse solutions, but skip first line containing the total number of lines
            for (int i = 1; i < lines.Length; i++) {
                solutions[i - 1] = Double.Parse(lines[i]);
            }
            return solutions;
        }
    }
}
