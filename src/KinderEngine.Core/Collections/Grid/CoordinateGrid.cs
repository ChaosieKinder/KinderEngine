using System;

namespace KinderEngine.Core.Collections.Grid
{
    /// <summary>
    /// A 2 dimensional square grid centered on 0,0 that supports negative indexes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CoordinateGrid<T>
    {
        private T[,] _grid;

        public uint Size { get; protected set; }

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public int Radius { get; protected set; }

        public CoordinateGrid(uint size)
        {
            if (size % 2 != 1)
                throw new ArgumentException("size must be odd.", nameof(size));
            Size = size;

            _grid = new T[Size, Size];
            Radius = (int)Math.Floor(Size / 2.0);
        }

        public T this[int x, int y]
        {
            get
            {
                var shiftedValueX = x + Radius;
                var shiftedValueY = y + Radius;
                return _grid[shiftedValueX, shiftedValueY];
            }
            set
            {
                var shiftedValueX = x + Radius;
                var shiftedValueY = y + Radius;
                _grid[shiftedValueX, shiftedValueY] = value;
            }
        }
    }
}
