namespace StackEverything.ObjectCopiers
{
    /// <summary>Copies things.</summary>
    internal interface ICopier<T>
    {
        T Copy(T obj);
    }
}