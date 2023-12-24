using System;

namespace UIXDialogBuilder
{
    public abstract class ReversibleMapperBase<TInner, TOuter> : IReversibleMapper<TInner, TOuter>
    {
        public Type InnerType => typeof(TInner);
        public Type OuterType => typeof(TOuter);
        public (Action<TOuter> setOuter, Func<TOuter> getOuter) Apply(Action<TInner> setInner, Func<TInner> getInner)
        {
            return (
                (outer) =>
                {
                    if (TryMapToInner(outer, out var inner))
                    {
                        setInner(inner);
                    }
                },
                () => TryMapToOuter(getInner(), out var outer) ? outer : default
            );
        }
        public abstract bool TryMapToInner(TOuter outer, out TInner inner);
        public abstract bool TryMapToOuter(TInner inner, out TOuter outer);
    }
}
