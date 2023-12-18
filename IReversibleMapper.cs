using System;

namespace UIXDialogBuilder
{
    public interface IReversibleMapper
    {
        Type InnerType { get; }
        Type OuterType { get; }
    }

    /// <summary>
    /// Allows mapping a value in both directions
    /// </summary>
    /// <typeparam name="TInner">type of the inner value</typeparam>
    /// <typeparam name="TOuter">type of the outer value</typeparam>
    public interface IReversibleMapper<TInner, TOuter> : IReversibleMapper
    {
        /// <summary>
        /// Tries to map an inner value
        /// </summary>
        /// <param name="inner">inner value</param>
        /// <param name="outer">mapped outer value if function returns true</param>
        /// <returns>true, if mapping is successful</returns>
        bool TryMapToOuter(TInner inner, out TOuter outer);

        /// <summary>
        /// Tries to map an outer value
        /// </summary>
        /// <param name="outer">outer value</param>
        /// <param name="inner">mapped inner value, if function returns true</param>
        /// <returns>true, if mapping is successful</returns>
        bool TryMapToInner(TOuter outer, out TInner inner);
        (Action<TOuter> setOuter, Func<TOuter> getOuter) Apply(Action<TInner> setInner, Func<TInner> getInner);
    }
}
