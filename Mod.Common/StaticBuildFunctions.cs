using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using System;
using System.Globalization;
using System.Reflection;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Users;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Interaction;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Slots;
using ProtoFlux.Core;

namespace UIXDialogBuilder
{
    //TODO: making it public?
    internal static class StaticBuildFunctions
    {
        private class MappedValue<TInner, TOuter>
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
                Inner = value;
            }
        }

        public static T BuildLineWithLabel<T>(string label, UIBuilder uiBuilder, Func<T> contentGen)
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

            var result = contentGen();

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
            IReversibleMapper<TInner, TOuter> mapper,
            IEditorGenerator<TOuter> editorGenerator)
        {
            (Action<TOuter> setOuter, Func<TOuter> getOuter) = mapper.Apply(setInner, getInner);
            return BuildEditor(uiBuilder, iFieldSlot, setOuter, getOuter, isSecret, name, customAttributes, editorGenerator);
        }

        internal static Action BuildEditor<T>(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<T> setInner,
            Func<T> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes,
            IEditorGenerator<T> editorGenerator)
        {
            Action reset;
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Static;
            var type = typeof(T);

            if (type.IsEnum && type.IsUnmanaged() && editorGenerator == null)
            {
                editorGenerator = (IEditorGenerator<T>)typeof(EnumEditorGenerator<>)
                    .MakeGenericType(typeof(T))
                    .GetConstructor(Array.Empty<Type>())
                    .Invoke(Array.Empty<object>());
            }

            if (editorGenerator != null)
            {
                reset = editorGenerator.Generate(uiBuilder, iFieldSlot, setInner, getInner, isSecret, name, customAttributes);
            }
            else if (typeof(Type) == type)
            {
                reset = (Action)typeof(StaticBuildFunctions)
                    .GetMethod(nameof(BuildTypeEditor))
                    .Invoke(null, new object[] { uiBuilder, iFieldSlot, setInner, getInner, name, customAttributes });
            }
            else if (typeof(IWorldElement).IsAssignableFrom(type))
            {
                reset = (Action)typeof(StaticBuildFunctions)
                    .GetGenericMethod(nameof(BuildReferenceEditor), FLAGS, type)
                    .Invoke(null, new object[] { uiBuilder, iFieldSlot, setInner, getInner, name, customAttributes });
            }
            else
            {
                reset = (Action)typeof(StaticBuildFunctions)
                    .GetGenericMethod(nameof(BuildValueEditor), FLAGS, type)
                    .Invoke(null, new object[] { uiBuilder, iFieldSlot, setInner, getInner, name, customAttributes });
            }

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

        private static Action BuildTypeEditor(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<Type> setInner,
            Func<Type> getInner,
            string name,
            ICustomAttributeProvider customAttributes)
        {
            var value = iFieldSlot.AttachComponent<TypeField>().Type;

            void reset()
            {
                value.Value = getInner();
            }

            reset();
            value.OnValueChange += (x) =>
            {
                setInner(x);
            };

            SyncMemberEditorBuilder.Build(
                value,
                null,
                new FieldInfoDecorator(typeof(TypeField).GetField(nameof(TypeField.Type)), customAttributes, name),
                uiBuilder
            );

            return reset;
        }

        private static Action BuildReferenceEditor<T>(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<T> setInner,
            Func<T> getInner,
            string name,
            ICustomAttributeProvider customAttributes) where T : class, IWorldElement
        {
            var value = iFieldSlot.AttachComponent<ReferenceField<T>>().Reference;

            void reset()
            {
                value.Target = getInner();
            }

            reset();
            value.OnTargetChange += (x) =>
            {
                setInner(x);
            };

            SyncMemberEditorBuilder.Build(
                value,
                null,
                new FieldInfoDecorator(typeof(ReferenceField<T>).GetField(nameof(ReferenceField<T>.Reference)), customAttributes, name),
                uiBuilder
            );
            return reset;
        }

        private static Action BuildValueEditor<T>(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<T> setInner,
            Func<T> getInner,
            string name,
            ICustomAttributeProvider customAttributes)
        {
            var value = iFieldSlot.AttachComponent<ValueField<T>>().Value;

            void reset()
            {
                value.Value = getInner();
            }

            reset();
            value.OnValueChange += (x) =>
            {
                setInner(x);
            };

            SyncMemberEditorBuilder.Build(
                value,
                null,
                new FieldInfoDecorator(typeof(ValueField<T>).GetField(nameof(ValueField<T>.Value)), customAttributes, name),
                uiBuilder
            );

            return reset;
        }

        public static void AddPrivateAction(this Button button, Action<User> onPressed)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (onPressed == null) throw new ArgumentNullException(nameof(onPressed));

            button.LocalPressed += (IButton b, ButtonEventData bed) =>
            {
                onPressed(b.World.LocalUser);
            };
            var localEnabled = button.Slot.AttachComponent<ValueUserOverride<bool>>();
            localEnabled.Target.Target = button.EnabledField;
            localEnabled.Default.Value = false;
            localEnabled.CreateOverrideOnWrite.Value = true;
        }

        public static void AddPublicAction(this IButton button, Action<User> onPressed)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (onPressed == null) throw new ArgumentNullException(nameof(onPressed));

            var protoflux = button.Slot.AddSlot("Protoflux");
            var template = button.Slot.AddSlot("Event");
            var queue = button.Slot.AddSlot("Queue");
            var userVar = template.AttachComponent<ReferenceField<User>>();
            var templateRef = protoflux.AttachComponent<RefObjectInput<Slot>>();
            templateRef.Target.Target = template;
            var queueRef = protoflux.AttachComponent<RefObjectInput<Slot>>();
            queueRef.Target.Target = queue;
            var localUser = protoflux.AttachComponent<LocalUser>();
            var userSource = protoflux.AttachComponent<ReferenceSource<User>>();
            var userSourceRef = protoflux.AttachComponent<GlobalReference<SyncRef<User>>>();
            userSource.Source.Target = userSourceRef;
            userSourceRef.Reference.Target = userVar.Reference;

            var buttonRef = protoflux.AttachComponent<GlobalReference<IButton>>();
            buttonRef.Reference.Target = button;
            var buttonEvents = protoflux.AttachComponent<ButtonEvents>();
            buttonEvents.Button.Target = buttonRef;
            var setUser = protoflux.AttachComponent<ObjectWrite<FrooxEngineContext, User>>();
            setUser.Value.Target = localUser;
            setUser.Variable.Target = userSource;
            var duplicate = protoflux.AttachComponent<DuplicateSlot>();
            duplicate.Template.Target = templateRef;
            var reparent = protoflux.AttachComponent<SetParent>();
            reparent.Instance.Target = duplicate.Duplicate;
            reparent.NewParent.Target = queueRef;

            buttonEvents.Pressed.Target = setUser;
            setUser.OnWritten.Target = duplicate;
            duplicate.Next.Target = reparent;

            queue.ChildAdded += (_, child) =>
            {
                child?.World?.RunSynchronously(() =>
                {
                    var user = child.GetComponent<ReferenceField<User>>()?.Reference.Target;
                    child.ReferenceID.ExtractIDs(out ulong position, out byte allocationID);
                    UniLog.Log($"{user} {child.World.GetUserByAllocationID(allocationID)}");
                    if (user != null && user == child.World.GetUserByAllocationID(allocationID))
                    {
                        onPressed(user);
                    }
                    child.Destroy();
                });
            };
        }

        public class FieldInfoDecorator : FieldInfo
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
