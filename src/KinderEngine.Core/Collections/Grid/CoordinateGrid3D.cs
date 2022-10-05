using System;

namespace KinderEngine.Core.Collections.Grid
{
    /// <summary>
    /// A 2 dimensional grid centered on 0,0 that supports negative indexes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CoordinateGrid3D<T>
    {
        public uint Width { get; }
        public uint Height { get; }

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public int Radius { get; }

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public int HeightRadius { get; }

        /// <summary>
        /// Creates a 3D grid with the specified parameters, with a center of (0,0,0)
        /// </summary>
        /// <param name="width">Width on each side (x/y) of the grid</param>
        /// <param name="height">Height of the grid</param>
        public CoordinateGrid3D(uint width, uint height)
        {
            if (width % 2 != 1)
                throw new ArgumentException("width must be odd.", nameof(width));
            if (height % 2 != 1)
                throw new ArgumentException("height must be odd.", nameof(height));
            Width = width;
            Height = height;
            Radius = (int)Math.Floor(Width / 2.0);
            HeightRadius = (int)Math.Floor(Height / 2.0);
            _grid = new T[Width, Width, Height];
        }


        public T this[int x, int y, int z]
        {
            get
            {
                return _grid[x + Radius,
                             y + Radius,
                             z + Radius];
            }
            set
            {
                _grid[x + Radius,
                      y + Radius,
                      z + Radius] = value;
            }
        }

        private T[,,] _grid;
    }
}
