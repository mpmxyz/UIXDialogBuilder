

using System;

namespace UIXDialogBuilder
{
    public static class GenericHelpers
    {
        public static bool HasConstructorFor(this Type constructedType, Type argumentType)
        {
            return constructedType?.GetConstructor(new Type[] { argumentType }) != null
                || constructedType?.GetConstructor(Type.EmptyTypes) != null;
        }

        public static object Construct(this Type constructedType, object valueOwner)
        {
            if (valueOwner == null) throw new ArgumentNullException(nameof(valueOwner));
            if (constructedType == null) throw new ArgumentNullException(nameof(constructedType));
            return constructedType.GetConstructor(new Type[] { valueOwner.GetType() })?.Invoke(new object[] { valueOwner })
                ?? constructedType.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>())
                ?? throw new InvalidOperationException($"{constructedType} lacks constructor with no argument or one matching type {valueOwner.GetType()}!");
        }
    }
}
