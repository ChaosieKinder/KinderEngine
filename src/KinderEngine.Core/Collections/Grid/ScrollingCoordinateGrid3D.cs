using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text;

namespace KinderEngine.Core.Collections.Grid
{
    /// <summary>
    /// A wrapper around CoordinateGrid<T> that supports scrolling and requesting
    /// loading/unloading of data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScrollingCoordinateGrid3D<T>
    {
        /// <summary>
        /// Has this grid been initialized to a start location?
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Dimensions of each side of the grid
        /// </summary>
        public uint Width => _grid.Width;

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public int Radius => _grid.Radius;

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public uint Height => _grid.Height;

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public int HeightRadius => _grid.HeightRadius;

        /// <summary>
        /// X Point where the grid is currently centered
        /// </summary>
        public int X => _offsetWorldX;

        /// <summary>
        /// Y Point where the grid is currently centered
        /// </summary>
        public int Y => _offsetWorldY;

        /// <summary>
        /// Y Point where the grid is currently centered
        /// </summary>
        public int Z => _offsetWorldZ;
        public int XMin { get; protected set; }
        public int YMin { get; protected set; }
        public int ZMin { get; protected set; }
        public int XMax { get; protected set; }
        public int YMax { get; protected set; }
        public int ZMax { get; protected set; }

        /// <summary>
        /// Queue of world data that needs to be unloaded
        /// </summary>
        public BlockingCollection<CoordinateGrid3DChangedEventArgs<T>> UnloadQueue { get; }

        /// <summary>
        /// Queue of world data that needs to be loaded
        /// </summary>
        public BlockingCollection<CoordinateGrid3DChangedEventArgs<T>> LoadQueue { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Should always be an odd number</param>
        public ScrollingCoordinateGrid3D(uint size, uint height)
        {
            if (size % 2 != 1)
                throw new ArgumentException("size must be odd.", nameof(size));
            if (height % 2 != 1)
                throw new ArgumentException("height must be odd.", nameof(size));

            _grid = new CoordinateGrid3D<T>(size, height);
            IsInitialized = false;
            // Set offsets to default such that default world location centers at (0,0)
            _offsetWorldX = 0;
            _offsetWorldY = 0; // same offset
            XMin = 0;
            YMin = 0;
            XMax = 0;
            YMax = 0;
            //initialize queues
            UnloadQueue = new BlockingCollection<CoordinateGrid3DChangedEventArgs<T>>();
            LoadQueue = new BlockingCollection<CoordinateGrid3DChangedEventArgs<T>>();
        }

        /// <summary>
        /// Scrolls the grid to center on the specified point. 
        ///     This is a locking action.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ScrollTo(int x, int y, int z)
        {
            // Determine Shift Amounts, our net change
            var worldShiftX = x - _offsetWorldX;
            var worldShiftY = y - _offsetWorldY;
            var worldShiftZ = z - _offsetWorldZ;

            // Old world Area Boundrys
            int owXmin = _offsetWorldX - Radius,
                owXmax = _offsetWorldX + Radius,
                owYmin = _offsetWorldY - Radius,
                owYmax = _offsetWorldY + Radius,
                owZmin = _offsetWorldY - HeightRadius,
                owZmax = _offsetWorldY + HeightRadius;

            // Set new offsets
            _offsetWorldX = x;
            _offsetWorldY = y;
            _offsetWorldZ = z;
            XMin = _offsetWorldX - Radius;
            YMin = _offsetWorldY - Radius;
            ZMin = _offsetWorldZ - HeightRadius;
            XMax = _offsetWorldX + Radius;
            YMax = _offsetWorldY + Radius;
            ZMax = _offsetWorldZ + HeightRadius;

            // New world area bounderys
            int nwXmin = _offsetWorldX - Radius,
                nwXmax = _offsetWorldX + Radius,
                nwYmin = _offsetWorldY - Radius,
                nwYmax = _offsetWorldY + Radius,
                nwZmin = _offsetWorldZ - HeightRadius,
                nwZmax = _offsetWorldZ + HeightRadius;

            // Itteration Directions
            var aXd = (worldShiftX >= 0 ? 1 : -1); // X itteration direction
            var aYd = (worldShiftY >= 0 ? 1 : -1); // Y itteration direction
            var aZd = (worldShiftZ >= 0 ? 1 : -1); // Y itteration direction

            // Lock durring item moving
            lock (_lock)
            {
                // We want to loop in the direction we're are moving in both loops, so that we can
                //   safely remove and move existing items and not have to worry about bulldozing data
                //   we have already processed.
                // gX and gY represent the grid's dimensions
                for (int ngZ = (int)(aZd == 1 ? -HeightRadius : HeightRadius);
                    (aZd == 1 ? ngZ <= HeightRadius : ngZ >= -HeightRadius); ngZ += aZd)
                {
                    for (int ngX = (int)(aXd == 1 ? -Radius : Radius);
                    (aXd == 1 ? ngX <= Radius : ngX >= -Radius); ngX += aXd)
                    {
                        for (int ngY = (int)(aYd == 1 ? -Radius : Radius);
                            (aYd == 1 ? ngY <= Radius : ngY >= -Radius); ngY += aYd)
                        {
                            // World location of this cell after movement
                            int nwX = ngX + _offsetWorldX,
                                nwY = ngY + _offsetWorldY,
                                nwZ = ngZ + _offsetWorldZ;
                            // World location of this cell before movement
                            int owX = nwX - worldShiftX,
                                owY = nwY - worldShiftY,
                                owZ = nwZ - worldShiftZ;
                            // gnX,gnY are the previous dimensions of this cell in the grid
                            //   IE: if this cell existed in the grid before movement,
                            //      it would be in this cell
                            int ogX = ngX + worldShiftX,
                                ogY = ngY + worldShiftY,
                                ogZ = ngZ + worldShiftZ;

                            //  Check if current cell is moving out of bounds.
                            if (owX < nwXmin || owX > nwXmax            // For Items out of bounds: pass event into 
                                || owY < nwYmin || owY > nwYmax
                                || owZ < nwZmin || owZ > nwZmax)       //   queue for processing removed items
                                UnloadQueue.Add(new CoordinateGrid3DChangedEventArgs<T>(this, owX, owY, owZ, _grid[ngX, ngY, ngZ]));

                            if (nwX >= owXmin && nwX <= owXmax
                                && nwY >= owYmin && nwY <= owYmax
                                && nwZ >= owZmin && nwZ <= owZmax)
                            {
                                // This cell after shifting is still inside the bounds of the shifted array,
                                // so we should be able to keep this data alive and just shift it.
                                _grid[ngX, ngY, ngZ] = _grid[ogX, ogY, ogZ];
                                _grid[ogX, ogY, ogZ] = default;
                            }
                            else
                            {
                                // (bx,by) is outside the existing cell, but will be inside it after the shift.
                                // Queue this data for loading
                                LoadQueue.Add(new CoordinateGrid3DChangedEventArgs<T>(this, nwX, nwY, nwZ, default));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialized the grid at the specified point.
        ///     This is a locking action.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void InitializeAt(int x, int y, int z)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Cannot initialize already initialized grid");
            IsInitialized = true;
            _offsetWorldX = x;
            _offsetWorldY = y; 
            _offsetWorldZ = z; // Set new offsets
            XMin = _offsetWorldX - Radius;
            YMin = _offsetWorldY - Radius;
            ZMin = _offsetWorldZ - HeightRadius;
            XMax = _offsetWorldX + Radius;
            YMax = _offsetWorldY + Radius;
            ZMax = _offsetWorldZ + HeightRadius;

            lock (_lock)
            {
                for (int az = z - HeightRadius; az <= z + HeightRadius; az++)
                    for (int ax = x - Radius; ax <= x + Radius; ax++)
                        for (int ay = y - Radius; ay <= y + Radius; ay++)
                            LoadQueue.Add(new CoordinateGrid3DChangedEventArgs<T>(this, ax, ay, az, default));
            }
        }

        /// <summary>
        /// Setting is locking, reading is not.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T this[int x, int y, int z]
        {
            get
            {
                var shiftedValueX = x - _offsetWorldX;
                var shiftedValueY = y - _offsetWorldY;
                var shiftedValueZ = z - _offsetWorldZ;
                if (shiftedValueX < -Radius
                    || shiftedValueX > Radius
                    || shiftedValueY < -Radius
                    || shiftedValueY > Radius
                    || shiftedValueZ < -HeightRadius
                    || shiftedValueZ > HeightRadius)
                    return default;
                return _grid[shiftedValueX, shiftedValueY, shiftedValueZ];
            }
            set
            {
                lock (_lock)
                {
                    var shiftedValueX = x - _offsetWorldX;
                    var shiftedValueY = y - _offsetWorldY;
                    var shiftedValueZ = z - _offsetWorldZ;
                    if (shiftedValueX >= -Radius
                        && shiftedValueX <= Radius
                        && shiftedValueY >= -Radius
                        && shiftedValueY <= Radius
                        && shiftedValueZ >= -HeightRadius
                        && shiftedValueZ <= HeightRadius)
                        _grid[shiftedValueX, shiftedValueY, shiftedValueZ] = value;
                }
            }
        }

        private int _offsetWorldX = 0;
        private int _offsetWorldY = 0;
        private int _offsetWorldZ = 0;
        private readonly CoordinateGrid3D<T> _grid;
        private object _lock = new object();
    }

    public class CoordinateGrid3DChangedEventArgs<T> : EventArgs
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public int Z { get; protected set; }
        public T ExistingItem { get; protected set; }
        public ScrollingCoordinateGrid3D<T> Grid { get; protected set; }
        public CoordinateGrid3DChangedEventArgs(ScrollingCoordinateGrid3D<T> grid, int x, int y, int z, T item = default) : base()
        {
            Grid = grid;
            X = x;
            Y = y;
            Z = z;
            ExistingItem = item;
        }
    }
}
