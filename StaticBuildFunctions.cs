using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Reflection;

namespace UIXDialogBuilder
{
    internal class StaticBuildFunctions
    {
        private class MappedValue<I, O> //TODO: ensure that reset works with mapping
        {
            internal O OValue;
            internal I IValue
            {
                get
                {
                    if (mapper.TryUnmap(OValue, out var iVal))
                    {
                        return iVal;
                    }
                    else
                    {
                        return default;
                    }
                }
                set
                {
                    if (mapper.TryMap(value, out var oVal))
                    {
                        OValue = oVal;
                    }
                }
            }
            private readonly IReversibleMapper<I, O> mapper;

            public MappedValue(I value, IReversibleMapper<I, O> mapper)
            {
                this.mapper = mapper;
                this.IValue = value;
            }
        }

        internal static T BuildLineWithLabel<T>(string label, UIBuilder uiBuilder, Func<UIBuilder, T> contentGen)
        {
            uiBuilder.PushStyle();
            uiBuilder.Style.MinHeight = 24f;
            uiBuilder.Panel();

            Text text2 = uiBuilder.Text(label + ":", bestFit: true, Alignment.MiddleLeft, parseRTF: false);
            text2.Color.Value = colorX.Black;
            uiBuilder.CurrentRect.AnchorMax.Value = new float2(0.25f, 1f);

            var rect = uiBuilder.Panel();

            rect.AnchorMin.Value = new float2(0.25f, 0f);
            var result = contentGen(uiBuilder);

            uiBuilder.NestOut();
            uiBuilder.NestOut();
            uiBuilder.PopStyle();

            return result;
        }

        internal static Action BuildEditor(Slot ifieldSlot, object valueObj, FieldInfo prop, Action onChange, UIBuilder uiBuilder, DialogOptionAttribute conf)
        {
            if (ifieldSlot == null) throw new ArgumentNullException(nameof(ifieldSlot));
            if (valueObj == null) throw new ArgumentNullException(nameof(valueObj));
            if (prop == null) throw new ArgumentNullException(nameof(prop));
            if (onChange == null) throw new ArgumentNullException(nameof(onChange));
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (conf == null) throw new ArgumentNullException(nameof(conf));

            if (conf.ToOutsideWorldMapper != null)
            {
                ApplyMapping(conf.ToOutsideWorldMapper, ref valueObj, ref prop, ref onChange);
            }
            (IField field, Action reset) = BuildField(ifieldSlot, valueObj, prop, onChange);
            SyncMemberEditorBuilder.Build(
                field,
                null,
                prop,
                uiBuilder
            );
            if (conf.Secret && uiBuilder.Current.ChildrenCount > 0)
            {
                Slot added = uiBuilder.Current;
                added.ForeachComponentInChildren<TextField>(textField =>
                    {
                        var patternField = textField.Text?.MaskPattern;
                        if (patternField != null)
                        {
                            patternField.Value = ModInstance.Current.SecretPatternText;
                        }
                    }
                );
            }
            return reset;
        }

        private static void ApplyMapping(Type reversibleMapper, ref object valueObj, ref FieldInfo prop, ref Action onChange)
        {
            //save original values to create adapters
            object originalObj = valueObj;
            FieldInfo originalProp = prop;
            Action originalOnChange = onChange;

            //create adapters
            Type[] mappingTypes = reversibleMapper.GetGenericArgumentsFromInterface(typeof(IReversibleMapper<,>));
            if (mappingTypes == null)
            {
                throw new ArgumentException("Expected implementation of " + typeof(IReversibleMapper<,>).Name, nameof(reversibleMapper)); //TODO: move to constructor
            }
            Type adaptedType = typeof(MappedValue<,>).MakeGenericType(mappingTypes);
            FieldInfo adaptedProp = adaptedType.GetField(nameof(MappedValue<object, object>.OValue), BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo iValueProp = adaptedType.GetProperty(nameof(MappedValue<object, object>.IValue), BindingFlags.NonPublic | BindingFlags.Instance);
            object mapperInstance = reversibleMapper.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
            object adaptedObj = adaptedType
                .GetConstructor(new Type[] { mappingTypes[0], typeof(IReversibleMapper<,>).MakeGenericType(mappingTypes) })
                .Invoke(new object[] { originalProp.GetValue(originalObj), mapperInstance });
            void adaptedOnChange()
            {
                object newValue = iValueProp.GetValue(adaptedObj);
                UniLog.Log(newValue);
                originalProp.SetValue(originalObj, newValue);
                originalOnChange();
            }

            //replace original values with adapters
            valueObj = adaptedObj;
            prop = adaptedProp;
            onChange = adaptedOnChange;
        }

        internal static void BuildSecretButton(UIBuilder uiBuilder, Action onClick)
        {
            Button button = uiBuilder.Button(ModInstance.Current.OpenSecretEditorTitle);
            button.LocalPressed += (b, d) =>
            {
                onClick();
            };
        }

        private static (IField field, Action reset) BuildField(Slot ifieldSlot, object valueObj, FieldInfo prop, Action onChange)
        {
            return ((IField field, Action reset))FieldBuilder(prop)
                ?.Invoke(null, new object[] { ifieldSlot, valueObj, prop, onChange });
        }

        private static MethodInfo FieldBuilder(FieldInfo prop)
        {
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Static;
            if (typeof(IWorldElement).IsAssignableFrom(prop.FieldType))
            {
                return typeof(StaticBuildFunctions)
                    .GetGenericMethod(nameof(BuildReferenceField), FLAGS, prop.FieldType);
            }
            else
            {
                return typeof(StaticBuildFunctions)
                    .GetGenericMethod(nameof(BuildValueField), FLAGS, prop.FieldType);
            }
        }

        private static (IField field, Action reset) BuildReferenceField<V>(Slot slot, object obj, FieldInfo prop, Action onChange) where V : class, IWorldElement
        {
            var value = slot.AttachComponent<ReferenceField<V>>().Reference;

            void reset()
            {
                value.Target = (V)prop.GetValue(obj);
            }

            reset();
            value.OnTargetChange += (x) =>
            {
                prop.SetValue(obj, x.Target);
                onChange();
            };
            return (value, reset);
        }

        private static (IField field, Action reset) BuildValueField<V>(Slot slot, object obj, FieldInfo prop, Action onChange)
        {
            var value = slot.AttachComponent<ValueField<V>>().Value;

            void reset()
            {
                value.Value = (V)prop.GetValue(obj);
            }

            reset();
            value.OnValueChange += (x) =>
            {
                prop.SetValue(obj, x.Value);
                onChange();
            };
            return (value, reset);
        }


    }
}
