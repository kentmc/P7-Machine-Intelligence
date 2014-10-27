using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLearning {
    class SequenceData {
        private List<int[]> sequences;
        int emptySequences;
        int symbols;

        public SequenceData(int symbols) {
            emptySequences = 0;
            this.symbols = symbols;
            sequences = new List<int[]>();
        }
        public void AddSequence(int[] seq) {
            sequences.Add(seq);
            if (seq.Length == 0)
                emptySequences++;
        }

        public int NumSymbols { get { return symbols; } }

        /// <summary>
        /// Returns a deep copy of all sequences
        /// </summary>
        /// <returns></returns>
        public int[][] GetSequences() {
            int[][] clonedSeq = new int[sequences.Count() - emptySequences][];
            int count = 0;
            foreach (int[] seq in sequences){
                if (seq.Length == 0)
                    continue;
                clonedSeq[count] = new int[seq.Length];
                for (int j = 0; j < seq.Length; j++)
                    clonedSeq[count][j] = seq[j];
                count++;
            }
            return clonedSeq;
        }

        public int Count { get { return sequences.Count; } }

        public int[] this[int i] {
            get { return sequences[i]; }
        }

        public void AddSequences(SequenceData sequenceData) {
            for (int i = 0; i < sequenceData.Count; i++)
                sequences.Add(sequenceData[i]);
            emptySequences += sequenceData.emptySequences;
        }
    }
}
