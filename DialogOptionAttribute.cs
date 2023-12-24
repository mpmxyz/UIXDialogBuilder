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
        public Type EditorGenerator { get; }

        public IReversibleMapper CreateMapper(object valueOwner)
        {
            return (IReversibleMapper)ToOutsideWorldMapper?.Construct(valueOwner);
        }

        public bool HasMapperFor(Type ownerType, Type valueType)
        {
            return ToOutsideWorldMapper.HasConstructorFor(ownerType)
                && ToOutsideWorldMapper.GetGenericArgumentsFromInterface(typeof(IReversibleMapper<,>))?[0] == valueType;
        }

        public object CreateUIGenerator<TValue>(object valueOwner)
        {
            return (IEditorGenerator<TValue>)EditorGenerator?.Construct(valueOwner);
        }

        public bool HasEditorGeneratorFor(Type ownerType, Type innerType)
        {
            return EditorGenerator.HasConstructorFor(ownerType)
                && EditorGenerator.GetGenericArgumentsFromInterface(typeof(IEditorGenerator<>))?[0] == ExpectedEditorType(innerType);
        }

        public Type ExpectedEditorType(Type innerType)
        {
            return ToOutsideWorldMapper?.GetGenericArgumentsFromInterface(typeof(IReversibleMapper<,>))?[1] ?? innerType;
        }


        /// <summary>
        /// Creates an option line in the dialog
        /// </summary>
        /// <param name="name">Display name of the option</param>
        /// <param name="secret">makes the user edit this in userspace</param>
        /// <param name="showErrors">causes some space below the input to be reserved for error messages.</param>
        /// <param name="toOutsideWorldMapper">allows editing custom types using an in-world representation using a non-custom type</param>
        /// <param name="editorGenerator"></param>
        public DialogOptionAttribute(string name, bool secret = false, bool showErrors = true, Type toOutsideWorldMapper = null, Type editorGenerator = null)
        {
            Name = name;
            Secret = secret;
            ShowErrors = showErrors;
            ToOutsideWorldMapper = toOutsideWorldMapper;
            EditorGenerator = editorGenerator;

            if (ToOutsideWorldMapper != null && ToOutsideWorldMapper.GetGenericArgumentsFromInterface(typeof(IReversibleMapper<,>)) == null)
            {
                throw new ArgumentException($"Mapper does not implement {typeof(IReversibleMapper<,>)}!", nameof(toOutsideWorldMapper));
            }
            if (EditorGenerator != null && EditorGenerator.GetGenericArgumentsFromInterface(typeof(IEditorGenerator<>)) == null)
            {
                throw new ArgumentException($"EditorGenerator does not implement {typeof(IEditorGenerator<>)}!", nameof(editorGenerator));
            }
        }
    }
}
