using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UIXDialogBuilder
{
    public class NonPrimitiveEditorGenerator<TValue> : IEditorGenerator<TValue>
    {

        public NonPrimitiveEditorGenerator()
        {
            if (typeof(TValue).IsPrimitive) throw new ArgumentException($"{typeof(TValue)} has to be non-primitive!", nameof(TValue));
        }

        public Action Generate(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<TValue> setInner,
            Func<TValue> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (iFieldSlot == null) throw new ArgumentNullException(nameof(iFieldSlot));
            if (setInner == null) throw new ArgumentNullException(nameof(setInner));
            if (getInner == null) throw new ArgumentNullException(nameof(getInner));
            if (customAttributes == null) throw new ArgumentNullException(nameof(customAttributes));

            Action reset = null;

            uiBuilder.VerticalLayout(4f);

            var elementNames = ((TupleElementNamesAttribute)customAttributes
                .GetCustomAttributes(typeof(TupleElementNamesAttribute), true)
                .FirstOrDefault())
                ?.TransformNames;

            int i = 0;
            foreach (var field in typeof(TValue).GetFields())
            {
                if (!field.IsInitOnly)
                {
                    var nameOverride = elementNames != null && i < elementNames.Count ? elementNames[i] : null;
                    reset += StaticBuildFunctions.BuildLineWithLabel(
                        nameOverride ?? field.Name,
                        uiBuilder,
                        () => (Action)GetType()
                        .GetGenericMethod(nameof(BuildEditor), BindingFlags.Static | BindingFlags.NonPublic, field.FieldType)
                        .Invoke(this, new object[] { uiBuilder, iFieldSlot, setInner, getInner, isSecret, name, customAttributes, field })
                    );
                }
                i++;
            }
            uiBuilder.NestOut();
            return reset;
        }

        private static Action BuildEditor<TField>(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<TValue> setInner,
            Func<TValue> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes,
            FieldInfo field)
        {
            return StaticBuildFunctions.BuildEditor(
                uiBuilder,
                iFieldSlot,
                (x) =>
                {
                    object innerValue = getInner();
                    field.SetValue(innerValue, x);
                    //The following line is necessary because valuetypes can only be edited when boxed or with more advanced trickery.
                    //It also ensures proper event handling within this library:
                    setInner((TValue)innerValue);
                },
                () => (TField)field.GetValue(getInner()),
                isSecret,
                name,
                customAttributes,
                null
            );
        }
    }
}
