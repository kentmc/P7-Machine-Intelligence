using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLearning {
    class SequenceData {
        private int[][] sequences;
        private int[][] non_empty_sequences;
        private List<int[]> sequence_list;
        private int emptySequences;
        private int symbols;

        public SequenceData(int symbols) {
            emptySequences = 0;
            this.symbols = symbols;
            sequence_list = new List<int[]>();
        }
        public void AddSequence(int[] seq) {
            sequence_list.Add(seq);
            if (seq.Length == 0)
                emptySequences++;
        }

        public int NumSymbols { get { return symbols; } }

        /// <summary>
        /// Converts all added sequences to an array
        /// </summary>
        public void Finalize() {
            sequences = sequence_list.ToArray();
            non_empty_sequences = sequence_list.Where(s => s.Length != 0).ToArray();
        }

        /// <summary>
        /// Returns a deep copy of all sequences
        /// </summary>
        /// <returns></returns>
        public int[][] GetAll() {
            return sequences;
        }

        public int Count {
            get { return sequences.GetLength(0); }
        }

        public int[] this[int i] {
            get {
                return sequences[i]; 
            }
        }

        public void AddSequences(SequenceData sequenceData) {
            for (int i = 0; i < sequenceData.Count; i++)
                sequence_list.Add(sequenceData[i]);
            emptySequences += sequenceData.emptySequences;
            Finalize();
        }

        internal int[][] GetNonempty() {
            return non_empty_sequences;
        }
    }
}
