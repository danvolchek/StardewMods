namespace StackEverything.ObjectCopiers
{
    /// <summary>Copies objects.</summary>
    internal interface ICopier<T>
    {
        /*********
        ** Methods
        *********/

        /// <summary>Copy the given object.</summary>
        /// <param name="obj">The object to copy</param>
        /// <returns>A copy of the object.</returns>
        T Copy(T obj);
    }
}
