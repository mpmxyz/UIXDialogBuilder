using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators;
using FrooxEngine.UIX;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace UIXDialogBuilder
{
    public class EnumEditorGenerator<TEnum> : IEditorGenerator<TEnum>
        where TEnum : unmanaged, Enum
    {
        private readonly Type enumType;
        private readonly ulong[] incrementingValues;
        private readonly ulong[] decrementingValues;
        private readonly (ulong, string)[] valueNames;

        public EnumEditorGenerator()
        {
            enumType = typeof(TEnum);
            var values = Enum.GetValues(enumType).Cast<TEnum>().Select(x => Convert.ToUInt64(x, CultureInfo.InvariantCulture)).ToArray();
            var names = Enum.GetNames(enumType);
            incrementingValues = values.Distinct().ToArray();
            decrementingValues = incrementingValues.Reverse().ToArray();
            valueNames = values.Select((x, i) => (x, names[i])).ToArray();
        }

        public Action Generate(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<TEnum> setInner,
            Func<TEnum> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (iFieldSlot == null) throw new ArgumentNullException(nameof(iFieldSlot));
            if (setInner == null) throw new ArgumentNullException(nameof(setInner));
            if (getInner == null) throw new ArgumentNullException(nameof(getInner));

            var underlyingField = iFieldSlot.AttachComponent<ValueField<ulong>>().Value;

            void reset()
            {
                underlyingField.Value = Convert.ToUInt64(getInner(), CultureInfo.InvariantCulture);
            }

            reset();

            underlyingField.OnValueChange += (x) =>
            {
                setInner((TEnum)Enum.ToObject(enumType, x));
            };

            if (enumType.GetCustomAttribute<FlagsAttribute>() == null)
            {
                uiBuilder.PushStyle();
                uiBuilder.HorizontalLayout(4f);
                uiBuilder.Style.MinHeight = ModInstance.Current.LineHeight;
                uiBuilder.Style.FlexibleWidth = -1f;
                uiBuilder.Style.MinWidth = ModInstance.Current.LineHeight;
                var decrementCycle = uiBuilder.Button((LocaleString)"<<").Slot.AttachComponent<ButtonValueCycle<ulong>>();
                decrementCycle.TargetValue.Target = underlyingField;
                foreach (var value in decrementingValues)
                {
                    decrementCycle.Values.Add(value);
                }
                uiBuilder.Style.FlexibleWidth = 100f;
                uiBuilder.Style.MinWidth = -1f;
                var valueDrive = uiBuilder.Button().Slot.AttachComponent<ValueOptionDescriptionDriver<ulong>>();
                //TODO: button action to spawn dropdown list
                valueDrive.Value.Target = underlyingField;
                valueDrive.Label.Target = valueDrive.Slot.GetComponentInChildren<Text>().Content;
                valueDrive.DefaultOption.Label.Value = "???";
                foreach ((var value, var valueName) in valueNames)
                {
                    var option = valueDrive.Options.Add();
                    option.ReferenceValue.Value = value;
                    option.Label.Value = valueName;
                }
                uiBuilder.Style.FlexibleWidth = -1f;
                uiBuilder.Style.MinWidth = ModInstance.Current.LineHeight;
                var incrementCycle = uiBuilder.Button((LocaleString)">>").Slot.AttachComponent<ButtonValueCycle<ulong>>();
                incrementCycle.TargetValue.Target = underlyingField;
                foreach (var value in incrementingValues)
                {
                    incrementCycle.Values.Add(value);
                }
                uiBuilder.NestOut();
                uiBuilder.PopStyle();
            }
            else
            {
                uiBuilder.PushStyle();
                uiBuilder.VerticalLayout(4f);
                uiBuilder.Style.MinHeight = ModInstance.Current.LineHeight;
                uiBuilder.Style.FlexibleWidth = 1f;
                foreach ((var value, var valueName) in valueNames)
                {
                    var checkbox = uiBuilder.Checkbox(valueName);
                    var protoFlux = checkbox.Slot.AddSlot("Protoflux");

                    var setter = checkbox.Slot.AttachComponent<ButtonValueCycle<ulong>>();
                    setter.TargetValue.Target = underlyingField;

                    var valueWithFlagSetter = setter.Values.Add();
                    var valueWithoutFlagSetter = setter.Values.Add();

                    var valueOfFlag = protoFlux.AttachComponent<ValueInput<ulong>>();
                    valueOfFlag.Value.Value = value;
                    var valueOfNotFlag = protoFlux.AttachComponent<ValueInput<ulong>>();
                    valueOfNotFlag.Value.Value = ~value;

                    var currentValueSource = protoFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueSource<ulong>>();
                    var currentValueReference = protoFlux.AttachComponent<GlobalReference<IValue<ulong>>>();
                    currentValueSource.Source.Target = currentValueReference;
                    currentValueReference.Reference.Target = underlyingField;

                    var valueWithFlag = protoFlux.AttachComponent<OR_Ulong>();
                    valueWithFlag.A.Target = currentValueSource;
                    valueWithFlag.B.Target = valueOfFlag;
                    var valueWithoutFlag = protoFlux.AttachComponent<AND_Ulong>();
                    valueWithoutFlag.A.Target = currentValueSource;
                    valueWithoutFlag.B.Target = valueOfNotFlag;

                    var driverWithFlag = protoFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueFieldDrive<ulong>>();
                    driverWithFlag.Value.Target = valueWithFlag;
                    var proxyWithFlag = protoFlux.AttachComponent<FrooxEngine.ProtoFlux.CoreNodes.FieldDriveBase<ulong>.Proxy>();
                    proxyWithFlag.Node.Target = driverWithFlag;
                    proxyWithFlag.Drive.Target = valueWithFlagSetter;

                    var driverWithoutFlag = protoFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueFieldDrive<ulong>>();
                    driverWithoutFlag.Value.Target = valueWithoutFlag;
                    var proxyWithoutFlag = protoFlux.AttachComponent<FrooxEngine.ProtoFlux.CoreNodes.FieldDriveBase<ulong>.Proxy>();
                    proxyWithoutFlag.Node.Target = driverWithoutFlag;
                    proxyWithoutFlag.Drive.Target = valueWithoutFlagSetter;

                    checkbox.State.DriveByEquality(underlyingField, default).Reference.DriveFrom(valueWithFlagSetter);
                }
                uiBuilder.NestOut();
                uiBuilder.PopStyle();
            }

            return reset;
        }
    }
}
