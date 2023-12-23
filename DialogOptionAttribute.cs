using Elements.Core;
using System;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Attributes fields representing a dialog option
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DialogOptionAttribute : Attribute
    {
        public string Name { get; }
        public bool Secret { get; }
        public bool ShowErrors { get; }
        public Type ToOutsideWorldMapper { get; }

        public IReversibleMapper CreateMapper(object valueOwner)
        {
            if (valueOwner == null) throw new ArgumentNullException(nameof(valueOwner));
            if (ToOutsideWorldMapper == null)
            {
                return null;
            }
            return (IReversibleMapper)ToOutsideWorldMapper.GetConstructor(new Type[] { valueOwner.GetType() })?.Invoke(new object[] { valueOwner })
                ?? (IReversibleMapper)ToOutsideWorldMapper.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>())
                ?? throw new InvalidOperationException($"{ToOutsideWorldMapper} lacks constructor with no argument or one matching type {valueOwner.GetType()}!");
        }

        public bool HasMapperFor(Type ownerType)
        {
            return (ToOutsideWorldMapper.GetConstructor(new Type[] { ownerType }) != null
                || ToOutsideWorldMapper.GetConstructor(Type.EmptyTypes) != null);
        }

        /// <summary>
        /// Creates an option line in the dialog
        /// </summary>
        /// <param name="name">Display name of the option</param>
        /// <param name="secret">makes the user edit this in userspace</param>
        /// <param name="showErrors">causes some space below the input to be reserved for error messages.</param>
        /// <param name="toOutsideWorldMapper">allows editing custom types using an in-world representation using a non-custom type</param>
        public DialogOptionAttribute(string name, bool secret = false, bool showErrors = true, Type toOutsideWorldMapper = null)
        {
            Name = name;
            Secret = secret;
            ShowErrors = showErrors;
            ToOutsideWorldMapper = toOutsideWorldMapper;
            if (ToOutsideWorldMapper != null && ToOutsideWorldMapper.GetGenericArgumentsFromInterface(typeof(IReversibleMapper<,>)) == null)
            {
                throw new ArgumentException($"Mapper does not implement {typeof(IReversibleMapper<,>)}!", nameof(toOutsideWorldMapper));
            }
        }
    }
}
