using System;
using System.Globalization;

namespace UIXDialogBuilder
{
    internal class ReversibleEnumMapper<TEnum> : ReversibleMapperBase<TEnum, ulong>
        where TEnum : unmanaged, Enum
    {
        private readonly Type enumType = typeof(TEnum);

        public override bool TryMapToInner(ulong outer, out TEnum inner)
        {
            inner = (TEnum)Enum.ToObject(enumType, outer);
            return true;
        }

        public override bool TryMapToOuter(TEnum inner, out ulong outer)
        {
            outer = Convert.ToUInt64(inner, CultureInfo.InvariantCulture);
            return true;
        }
    }
}
