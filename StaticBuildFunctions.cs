using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Globalization;
using System.Reflection;

namespace UIXDialogBuilder
{
    internal class StaticBuildFunctions
    {
        private class MappedValue<TInner, TOuter> //TODO: ensure that reset works with mapping
        {
            internal TOuter Outer;
            internal TInner Inner
            {
                get
                {
                    if (mapper.TryMapToInner(Outer, out var iVal))
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
                    if (mapper.TryMapToOuter(value, out var oVal))
                    {
                        Outer = oVal;
                    }
                }
            }
            private readonly IReversibleMapper<TInner, TOuter> mapper;

            public MappedValue(TInner value, IReversibleMapper<TInner, TOuter> mapper)
            {
                this.mapper = mapper;
                this.Inner = value;
            }
        }

        internal static T BuildLineWithLabel<T>(string label, UIBuilder uiBuilder, Func<UIBuilder, T> contentGen)
        {
            uiBuilder.PushStyle();

            uiBuilder.Style.MinHeight = ModInstance.Current.LineHeight;
            uiBuilder.HorizontalLayout(4f);

            uiBuilder.Style.FlexibleWidth = 0.25f;
            uiBuilder.Style.PreferredWidth = 0f;
            uiBuilder.Style.MinWidth = 0f;
            uiBuilder.Style.UseZeroMetrics = true;

            uiBuilder.Style.TextAutoSizeMax = ModInstance.Current.LineHeight;
            uiBuilder.Style.PreferredHeight = ModInstance.Current.LineHeight;
            uiBuilder.Text(label + ":", bestFit: true, Alignment.MiddleLeft, parseRTF: false);
            uiBuilder.CurrentRect.AnchorMax.Value = new float2(0.25f, 1f);

            uiBuilder.Style.PreferredHeight = -1f;
            uiBuilder.Style.FlexibleWidth = 0.75f;
            uiBuilder.HorizontalLayout();

            uiBuilder.PopStyle();

            var result = contentGen(uiBuilder);

            uiBuilder.NestOut();
            uiBuilder.NestOut();

            return result;
        }

        internal static Action BuildEditorWithMapping<TInner, TOuter>(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<TInner> setInner,
            Func<TInner> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes,
            IReversibleMapper<TInner, TOuter> mapper)
        {
            (Action<TOuter> setOuter, Func<TOuter> getOuter) = mapper.Apply(setInner, getInner);
            return BuildEditor(uiBuilder, iFieldSlot, setOuter, getOuter, isSecret, name, customAttributes);
        }

        internal static Action BuildEditor<T>(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<T> setInner,
            Func<T> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes)
        {
            (FieldInfo fieldInfo, IField field, Action reset) = BuildField(iFieldSlot, setInner, getInner);
            SyncMemberEditorBuilder.Build(
                field,
                null,
                new FieldInfoDecorator(fieldInfo, customAttributes, name),
                uiBuilder
            );
            if (isSecret && uiBuilder.Current.ChildrenCount > 0)
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

        internal static void BuildSecretButton(UIBuilder uiBuilder, Action onClick)
        {
            uiBuilder.PushStyle();
            uiBuilder.Style.MinHeight = ModInstance.Current.LineHeight;
            Button button = uiBuilder.Button(ModInstance.Current.OpenSecretEditorTitle);
            button.LocalPressed += (b, d) =>
            {
                onClick();
            };
            uiBuilder.PopStyle();
        }

        private static (FieldInfo fieldInfo, IField field, Action reset) BuildField<T>(Slot iFieldSlot, Action<T> setInner, Func<T> getInner)
        {
            return ((FieldInfo fieldInfo, IField field, Action reset))FieldBuilder(typeof(T)).Invoke(null, new object[] { iFieldSlot, setInner, getInner });
        }

        private static MethodInfo FieldBuilder(Type type)
        {
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Static;
            if (typeof(IWorldElement).IsAssignableFrom(type))
            {
                return typeof(StaticBuildFunctions)
                    .GetGenericMethod(nameof(BuildReferenceField), FLAGS, type);
            }
            else
            {
                return typeof(StaticBuildFunctions)
                    .GetGenericMethod(nameof(BuildValueField), FLAGS, type);
            }
        }

        private static (FieldInfo fieldInfo, IField field, Action reset) BuildReferenceField<T>(Slot slot, Action<T> setInner, Func<T> getInner) where T : class, IWorldElement
        {
            var value = slot.AttachComponent<ReferenceField<T>>().Reference;

            void reset()
            {
                value.Target = getInner();
            }

            reset();
            value.OnTargetChange += (x) =>
            {
                setInner(x);
            };

            return (typeof(ReferenceField<T>).GetField(nameof(ReferenceField<T>.Reference)), value, reset);
        }

        private static (FieldInfo fieldInfo, IField field, Action reset) BuildValueField<T>(Slot slot, Action<T> setInner, Func<T> getInner)
        {
            var value = slot.AttachComponent<ValueField<T>>().Value;

            void reset()
            {
                value.Value = getInner();
            }

            reset();
            value.OnValueChange += (x) =>
            {
                setInner(x);
            };

            return (typeof(ValueField<T>).GetField(nameof(ValueField<T>.Value)), value, reset);
        }

        private class FieldInfoDecorator : FieldInfo
        {
            private readonly FieldInfo field;
            private readonly ICustomAttributeProvider customAttributes;
            private readonly string name;

            public FieldInfoDecorator(FieldInfo field, ICustomAttributeProvider customAttributes, string name)
            {
                this.field = field;
                this.customAttributes = customAttributes;
                this.name = name;
            }

            public override RuntimeFieldHandle FieldHandle => field.FieldHandle;

            public override Type FieldType => field.FieldType;

            public override FieldAttributes Attributes => field.Attributes;

            public override string Name => name;

            public override Type DeclaringType => field.DeclaringType;

            public override Type ReflectedType => field.ReflectedType;

            public override object[] GetCustomAttributes(bool inherit)
            {
                return customAttributes.GetCustomAttributes(inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return customAttributes.GetCustomAttributes(attributeType, inherit);
            }

            public override object GetValue(object obj)
            {
                return field.GetValue(obj);
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return customAttributes.IsDefined(attributeType, inherit);
            }

            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
            {
                field.SetValue(obj, value, invokeAttr, binder, culture);
            }
        }
    }

}
