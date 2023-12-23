using System;

namespace UIXDialogBuilder
{
    //TODO: annotation to create custom UI

    internal class ReversibleEnumMapper<T> : ReversibleMapperBase<T, long> where T : Enum
    {
        public override bool TryMapToInner(long outer, out T inner)
        {
            //TODO: flag handling (combination of constants -> IsDefined fails)
            if (Enum.IsDefined(typeof(T), outer))
            {
                inner = (T)(object)outer;
                return true;
            }
            else
            {
                inner = default;
                return false;
            }
        }

        public override bool TryMapToOuter(T inner, out long outer)
        {
            outer = (long)(object)inner;
            return true;
        }
    }
}
