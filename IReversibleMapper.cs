namespace UIXDialogBuilder
{
    /// <summary>
    /// Allows mapping a value and getting the original value from a mapped value
    /// </summary>
    /// <typeparam name="TInner"></typeparam>
    /// <typeparam name="TOuter"></typeparam>
    public interface IReversibleMapper<TInner, TOuter>
    {
        /// <summary>
        /// Tries to map the value
        /// </summary>
        /// <param name="value">original value</param>
        /// <param name="mapped">mapped value if function return strue</param>
        /// <returns>true, if mapping is successful</returns>
        bool TryMap(TInner value, out TOuter mapped);

        /// <summary>
        /// Tries to determine original value
        /// </summary>
        /// <param name="value">mapped value</param>
        /// <param name="unmapped">original value, if function returns true</param>
        /// <returns>true, if rever</returns>
        bool TryUnmap(TOuter value, out TInner unmapped);
    }
}
