namespace KinderEngine.Core.Collections.Threading
{
    public interface IExecutionQueue<T>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Required when using generic type")]
        void Enqueue(T obj);
    }
}
