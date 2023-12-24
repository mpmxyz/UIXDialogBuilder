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
    public class EnumEditorGenerator<TEnum> : IEditorGenerator<ulong>
        where TEnum : unmanaged, Enum
    {
        private readonly ulong[] incrementingValues;
        private readonly ulong[] decrementingValues;
        private readonly (ulong, string)[] valueNames;

        public EnumEditorGenerator() {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => Convert.ToUInt64(x, CultureInfo.InvariantCulture)).ToArray();
            var names = Enum.GetNames(typeof(TEnum));
            incrementingValues = values.Distinct().ToArray();
            decrementingValues = incrementingValues.Reverse().ToArray();
            valueNames = values.Select((x, i) => (x, names[i])).ToArray();
        }

        public void Generate(UIBuilder uiBuilder, IField field, ICustomAttributeProvider customAttributes)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (!(field is IField<ulong> underlyingField)) throw new ArgumentException($"A field of type {typeof(IField<ulong>)} is required!", nameof(field));

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() == null)
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
                valueDrive.Value.Target = underlyingField;
                valueDrive.Label.Target = valueDrive.Slot.GetComponentInChildren<Text>().Content;
                valueDrive.DefaultOption.Label.Value = "???";
                foreach ((var value, var name) in valueNames)
                {
                    var option = valueDrive.Options.Add();
                    option.ReferenceValue.Value = value;
                    option.Label.Value = name;
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
                foreach ((var value, var name) in valueNames)
                {
                    var checkbox = uiBuilder.Checkbox(name);
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
        }
    }
}
