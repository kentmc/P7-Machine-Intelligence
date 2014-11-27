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
        /// This method should call every time sequences have been added, as it converts all the sequences to an array
        /// </summary>
        public void SaveAddedSequences() {
            sequences = sequence_list.ToArray();
            non_empty_sequences = sequence_list.Where(s => s.Length != 0).ToArray();
        }

        /// <summary>
        /// Returns all sequences
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
            SaveAddedSequences();
        }

        internal int[][] GetNonempty() {
            return non_empty_sequences;
        }

        public Tuple<SequenceData, SequenceData> RandomSplit(double ratio) {
            SequenceData part1 = new SequenceData(NumSymbols);
            SequenceData part2 = new SequenceData(NumSymbols);
            List<int[]> shuffled = sequence_list.Select(e => e).ToList();
            Utilities.Shuffle(shuffled);
            int size_part1 = (int)(shuffled.Count * ratio);
            for (int i = 0; i < shuffled.Count; i++) {
                if (i < size_part1)
                    part1.AddSequence(shuffled[i]);
                else
                    part2.AddSequence(shuffled[i]);
            }
            part1.SaveAddedSequences();
            part2.SaveAddedSequences();
            return new Tuple<SequenceData, SequenceData>(part1, part2);
        }

        
    }
}
