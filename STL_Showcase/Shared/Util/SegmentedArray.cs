using STL_Showcase.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Shared.Util
{
    public class SegmentedArray<T> : IEnumerable<T>
    {

        const int defaultSegmentSize = 10000;
        int segmentSize;
        public int Length { get; private set; }

        T[][] array;

        public SegmentedArray(int length, int segmentSize = defaultSegmentSize)
        {
            this.segmentSize = segmentSize;
            this.Length = length;
            int mainSize = length > segmentSize ? (int)Math.Ceiling((float)length / (float)defaultSegmentSize) : 1;

            array = new T[mainSize][];

            for (int i = 0; i < array.Length; i += 1)
            {
                array[i] = new T[i * segmentSize > length - segmentSize ? (length - segmentSize * i) : segmentSize];
            }
        }

        public T this[int key] {
            get => array[key / segmentSize][key % segmentSize];
            set => array[key / segmentSize][key % segmentSize] = value;
        }



        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < array.Length; i++)
                for (int j = 0; j < array[i].Length; j++)
                    yield return array[i][j];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < array.Length; i++)
                for (int j = 0; j < array[i].Length; j++)
                    yield return array[i][j];
        }

        public static implicit operator SegmentedArray<T>(SegmentedArray<Half> v)
        {
            throw new NotImplementedException();
        }
    }
}
