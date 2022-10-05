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
    public class ScrollingCoordinateGrid<T>
    {
        /// <summary>
        /// Has this grid been initialized to a start location?
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Dimensions of each side of the grid
        /// </summary>
        public uint Size => _grid.Size;

        /// <summary>
        /// How much space on a side from the center
        /// </summary>
        public int Radius => _grid.Radius;

        /// <summary>
        /// X Point where the grid is currently centered
        /// </summary>
        public int X => _offsetWorldX;

        /// <summary>
        /// Y Point where the grid is currently centered
        /// </summary>
        public int Y => _offsetWorldY;

        public int XMin { get; protected set; }
        public int YMin { get; protected set; }
        public int XMax { get; protected set; }
        public int YMax { get; protected set; }

        /// <summary>
        /// Queue of world data that needs to be unloaded
        /// </summary>
        public BlockingCollection<CoordinateGridChangedEventArgs<T>> UnloadQueue { get; private set; }

        /// <summary>
        /// Queue of world data that needs to be loaded
        /// </summary>
        public BlockingCollection<CoordinateGridChangedEventArgs<T>> LoadQueue { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">Should always be an odd number</param>
        public ScrollingCoordinateGrid(uint size)
        {
            if (size % 2 != 1)
                throw new ArgumentException("size must be odd.", nameof(size));

            _grid = new CoordinateGrid<T>(size);
            IsInitialized = false;
            // Set offsets to default such that default world location centers at (0,0)
            _offsetWorldX = 0;
            _offsetWorldY = 0; // same offset
            XMin = 0;
            YMin = 0;
            XMax = 0;
            YMax = 0;
            //initialize queues
            UnloadQueue = new BlockingCollection<CoordinateGridChangedEventArgs<T>>();
            LoadQueue = new BlockingCollection<CoordinateGridChangedEventArgs<T>>();
        }

        /// <summary>
        /// Scrolls the grid to center on the specified point. 
        ///     This is a locking action.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ScrollTo(int x, int y)
        {
            // Determine Shift Amounts, our net change
            var worldShiftX = x - _offsetWorldX;
            var worldShiftY = y - _offsetWorldY;

            // Old world Area Boundrys
            int owXmin = _offsetWorldX - Radius,
                owXmax = _offsetWorldX + Radius,
                owYmin = _offsetWorldY - Radius,
                owYmax = _offsetWorldY + Radius;

            // Set new offsets
            _offsetWorldX = x;
            _offsetWorldY = y;
            XMin = _offsetWorldX - Radius;
            YMin = _offsetWorldY - Radius;
            XMax = _offsetWorldX + Radius;
            YMax = _offsetWorldY + Radius;

            // New world area bounderys
            int nwXmin = _offsetWorldX - Radius,
                nwXmax = _offsetWorldX + Radius,
                nwYmin = _offsetWorldY - Radius,
                nwYmax = _offsetWorldY + Radius;

            // Itteration Directions
            var aXd = (worldShiftX >= 0 ? 1 : -1); // X itteration direction
            var aYd = (worldShiftY >= 0 ? 1 : -1); // Y itteration direction

            // Lock durring item moving
            lock (_lock)
            {
                // We want to loop in the direction we're are moving in both loops, so that we can
                //   safely remove and move existing items and not have to worry about bulldozing data
                //   we have already processed.
                // gX and gY represent the grid's dimensions
                for (int ngX = (int)(aXd == 1 ? -Radius : Radius);
                    (aXd == 1 ? ngX <= Radius : ngX >= -Radius); ngX += aXd)
                {
                    for (int ngY = (int)(aYd == 1 ? -Radius : Radius);
                        (aYd == 1 ? ngY <= Radius : ngY >= -Radius); ngY += aYd)
                    {
                        // World location of this cell after movement
                        int nwX = ngX + _offsetWorldX,
                            nwY = ngY + _offsetWorldY;
                        // World location of this cell before movement
                        int owX = nwX - worldShiftX,
                            owY = nwY - worldShiftY;
                        // gnX,gnY are the previous dimensions of this cell in the grid
                        //   IE: if this cell existed in the grid before movement,
                        //      it would be in this cell
                        int ogX = ngX + worldShiftX,
                            ogY = ngY + worldShiftY;

                        //  Check if current cell is moving out of bounds.
                        if (owX < nwXmin || owX > nwXmax            // For Items out of bounds: pass event into 
                            || owY < nwYmin || owY > nwYmax)       //   queue for processing removed items
                            UnloadQueue.Add(new CoordinateGridChangedEventArgs<T>(this, owX, owY, _grid[ngX, ngY]));

                        if (nwX >= owXmin && nwX <= owXmax
                            && nwY >= owYmin && nwY <= owYmax)
                        {
                            // This cell after shifting is still inside the bounds of the shifted array,
                            // so we should be able to keep this data alive and just shift it.
                            _grid[ngX, ngY] = _grid[ogX, ogY];
                            _grid[ogX, ogY] = default;
                        }
                        else
                        {
                            // (bx,by) is outside the existing cell, but will be inside it after the shift.
                            // Queue this data for loading
                            LoadQueue.Add(new CoordinateGridChangedEventArgs<T>(this, nwX, nwY, default));
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
        public void InitializeAt(int x, int y)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Cannot initialize already initialized grid");
            IsInitialized = true;
            _offsetWorldX = x;
            _offsetWorldY = y; // Set new offsets
            XMin = _offsetWorldX - Radius;
            YMin = _offsetWorldY - Radius;
            XMax = _offsetWorldX + Radius;
            YMax = _offsetWorldY + Radius;

            lock (_lock)
            {
                for (int ax = x - Radius; ax <= x + Radius; ax++)
                    for (int ay = y - Radius; ay <= y + Radius; ay++)
                        LoadQueue.Add(new CoordinateGridChangedEventArgs<T>(this, ax, ay, default));
            }
        }

        /// <summary>
        /// Setting is locking, reading is not.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T this[int x, int y]
        {
            get
            {
                var shiftedValueX = x - _offsetWorldX;
                var shiftedValueY = y - _offsetWorldY;
                if (shiftedValueX < - Radius
                    || shiftedValueX > Radius
                    || shiftedValueY < - Radius
                    || shiftedValueY > Radius)
                    return default;
                return _grid[shiftedValueX, shiftedValueY];
            }
            set
            {
                lock (_lock)
                {
                    var shiftedValueX = x - _offsetWorldX;
                    var shiftedValueY = y - _offsetWorldY;
                    if (shiftedValueX >= - Radius
                        && shiftedValueX <= Radius
                        && shiftedValueY >= - Radius
                        && shiftedValueY <= Radius)
                        _grid[shiftedValueX, shiftedValueY] = value;
                }
            }
        }

        private int _offsetWorldX = 0;
        private int _offsetWorldY = 0;
        private readonly CoordinateGrid<T> _grid;
        private object _lock = new object();
    }

    public class CoordinateGridChangedEventArgs<T> : EventArgs
    {
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public T ExistingItem { get; protected set; }
        public ScrollingCoordinateGrid<T> Grid { get; protected set; }
        public CoordinateGridChangedEventArgs(ScrollingCoordinateGrid<T> grid, int x, int y, T item = default) : base()
        {
            Grid = grid;
            X = x;
            Y = y;
            ExistingItem = item;
        }
    }
}
