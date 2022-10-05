using System.Runtime.InteropServices;

namespace KinderEngine.Core.Generic.Internals
{
    public enum PendingEventType
    {
        Add,
        Insert,
        Remove,
        RemoveAt,
        Replace,
        Clear,
    }

    public static class PendingEvent
    {
        public static PendingEvent<T> Add<T>(T item) => new PendingEvent<T>(PendingEventType.Add, item);

        public static PendingEvent<T> Insert<T>(int index, T item) => new PendingEvent<T>(PendingEventType.Insert, item, index);

        public static PendingEvent<T> Remove<T>(T item) => new PendingEvent<T>(PendingEventType.Remove, item);

        public static PendingEvent<T> RemoveAt<T>(int index) => new PendingEvent<T>(PendingEventType.RemoveAt, index);

        public static PendingEvent<T> Replace<T>(int index, T item) => new PendingEvent<T>(PendingEventType.Replace, item, index);

        public static PendingEvent<T> Clear<T>() => new PendingEvent<T>(PendingEventType.Clear);
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly struct PendingEvent<T>
    {
        public PendingEvent(PendingEventType type)
        {
            Type = type;
            Item = default!;
            Index = -1;
        }

        public PendingEvent(PendingEventType type, int index)
        {
            Type = type;
            Item = default!;
            Index = index;
        }

        public PendingEvent(PendingEventType type, T item)
        {
            Type = type;
            Item = item;
            Index = -1;
        }

        public PendingEvent(PendingEventType type, T item, int index)
        {
            Type = type;
            Item = item;
            Index = index;
        }

        public PendingEventType Type { get; }
        public T Item { get; }
        public int Index { get; }
    }
}
