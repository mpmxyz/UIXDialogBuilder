using FrooxEngine;
using System.Collections.Generic;
using FrooxEngine.UIX;
using System.Reflection;
using System;
using Elements.Core;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators;
using FrooxEngine.ProtoFlux;
using FrooxEngine.Undo;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Math;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Helper class to create an import dialog <br/>
    /// To be exact this class starts its life as a builder where you can add defaults or custom elements to define a dialog.
    /// Then you can use it as a factory to produce new UIX windows from objects representing your custom dialog.
    /// </summary>
    /// <typeparam name="TDialogState">expected dialog object type</typeparam>
    public partial class DialogBuilder<TDialogState> where TDialogState : IDialogState
    {
        private readonly List<IDialogEntryDefinition<TDialogState>> definitions = new List<IDialogEntryDefinition<TDialogState>>();
        private readonly Action<object> onInputOverride;

        /// <summary>
        /// Creates a dialog builder that can be configured to create dialog windows.
        /// </summary>
        /// <param name="addDefaults">creates a list of options, an output for errors and a line with buttons based on <typeparamref name="TDialogState"/>'s attributes</param>
        public DialogBuilder(bool addDefaults = true, Action<object> onInputOverride = null)
        {
            if (addDefaults)
            {
                AddAllOptions();
                AddUnboundErrorDisplay();
                AddAllActions();
            }

            this.onInputOverride = onInputOverride;
        }

        /// <summary>
        /// Adds a line to the dialog configuration
        /// </summary>
        /// <param name="optionField">object that will generate UI for an instance of <typeparamref name="TDialogState"/></param>
        /// <returns>this</returns>
        public DialogBuilder<TDialogState> AddEntry(IDialogEntryDefinition<TDialogState> optionField)
        {
            definitions.Add(optionField);
            return this;
        }

        /// <summary>
        /// Adds a line for each of <typeparamref name="TDialogState"/>'s attributes annotated with <see cref="DialogOptionAttribute"/>
        /// </summary>
        /// <returns>this</returns>
        public DialogBuilder<TDialogState> AddAllOptions()
        {
            var dialogStateType = typeof(TDialogState);

            foreach (var fieldInfo in dialogStateType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var conf = fieldInfo.GetCustomAttribute<DialogOptionAttribute>();
                if (conf != null)
                {
                    AddOption(fieldInfo, conf);
                }
            }
            foreach (var propInfo in dialogStateType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var conf = propInfo.GetCustomAttribute<DialogOptionAttribute>();
                if (conf != null)
                {
                    AddOption(propInfo, conf);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a line with one button for each of <typeparamref name="TDialogState"/>'s attributes annotated with <see cref="DialogActionAttribute"/>
        /// </summary>
        /// <returns>this</returns>
        /// <exception cref="InvalidOperationException">If an annotated method has arguments.</exception>
        public DialogBuilder<TDialogState> AddAllActions()
        {
            var dialogStateType = typeof(TDialogState);

            var actions = new List<DialogActionDefinition<TDialogState>>();
            foreach (var methodInfo in dialogStateType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attr in methodInfo.GetCustomAttributes(true))
                {
                    if (attr is DialogActionAttribute conf)
                    {
                        if (methodInfo.GetParameters().Length != 0)
                        {
                            throw new InvalidOperationException($"DialogAction '{methodInfo.Name}' must have no arguments!");
                        }
                        actions.Add(new DialogActionDefinition<TDialogState>(methodInfo.Name, conf, (dialog) => methodInfo.Invoke(dialog, Array.Empty<object>())));
                        break;
                    }
                }
            }
            AddLine(null, actions);

            return this;
        }

        /// <summary>
        /// Adds a line with an editable value
        /// </summary>
        /// <param name="conf">displayed name, secrecy and error output options</param>
        /// <param name="fieldInfo">field of <typeparamref name="TDialogState"/> which will be edited</param>
        /// <returns>this</returns>
        public DialogBuilder<TDialogState> AddOption(FieldInfo fieldInfo, DialogOptionAttribute conf)
        {
            if (conf == null) throw new ArgumentNullException(nameof(conf));
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));
            if (!fieldInfo.DeclaringType.IsAssignableFrom(typeof(TDialogState)))
            {
                throw new ArgumentException($"Field {fieldInfo.Name} must be part of {typeof(TDialogState)}!", nameof(fieldInfo));
            }

            var genType = typeof(DialogOptionDefinition<,>).MakeGenericType(typeof(TDialogState), fieldInfo.FieldType);
            var cons = genType.GetConstructor(
                    new Type[] {
                        typeof(FieldInfo),
                        typeof(DialogOptionAttribute)
                    }
                );
            var field = (IDialogEntryDefinition<TDialogState>)cons.Invoke(new object[] { fieldInfo, conf });
            return AddEntry(field);
        }

        /// <summary>
        /// Adds a line with an editable value
        /// </summary>
        /// <param name="conf">displayed name, secrecy and error output options</param>
        /// <param name="propInfo">property of <typeparamref name="TDialogState"/> which will be edited</param>
        /// <returns>this</returns>
        public DialogBuilder<TDialogState> AddOption(PropertyInfo propInfo, DialogOptionAttribute conf)
        {
            if (conf == null) throw new ArgumentNullException(nameof(conf));
            if (propInfo == null) throw new ArgumentNullException(nameof(propInfo));
            if (!propInfo.DeclaringType.IsAssignableFrom(typeof(TDialogState)))
            {
                throw new ArgumentException($"Property {propInfo.Name} must be part of {typeof(TDialogState)}!", nameof(propInfo));
            }

            var genType = typeof(DialogOptionDefinition<,>).MakeGenericType(typeof(TDialogState), propInfo.PropertyType);
            var cons = genType.GetConstructor(
                    new Type[] {
                        typeof(FieldInfo),
                        typeof(DialogOptionAttribute)
                    }
                );
            var field = (IDialogEntryDefinition<TDialogState>)cons.Invoke(new object[] { propInfo, conf });
            return AddEntry(field);
        }

        /// <summary>
        /// Adds a line with multiple sub-elements
        /// </summary>
        /// <param name="elements">Elements that will be placed in a single line.</param>
        /// <returns>this</returns>
        public DialogBuilder<TDialogState> AddLine(object key, IEnumerable<IDialogEntryDefinition<TDialogState>> elements)
        {
            AddEntry(new DialogLineDefinition<TDialogState>(key, elements));
            return this;
        }

        /// <summary>
        /// Creates a text output that shows a list of all errors that are not displayed directly on the problematic input.
        /// </summary>
        /// <returns>this</returns>
        public DialogBuilder<TDialogState> AddUnboundErrorDisplay(object key = null, int nLines = 2)
        {
            AddEntry(new DialogErrorDisplayDefinition<TDialogState>(key, onlyUnbound: true, nLines: nLines));
            return this;
        }

        /// <summary>
        /// Adds the dialog UI to whereever the <paramref name="uiBuilder"/> is currently at
        /// </summary>
        /// <param name="uiBuilder">used to build the UI</param>
        /// <param name="dialogState">object that will be configured by the UI</param>
        /// <param name="parent">parent dialog</param>
        public Dialog BuildInPlace(UIBuilder uiBuilder, TDialogState dialogState, Dialog parent = null)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (dialogState == null) throw new ArgumentNullException(nameof(dialogState));

            var dialogRoot = uiBuilder.Current;

            uiBuilder.PushStyle();
            RadiantUI_Constants.SetupEditorStyle(uiBuilder);

            var elements = new List<IDialogElement>();
            var boundErrorKeys = new HashSet<object>();
            var world = uiBuilder.World;
            var inUserspace = world.IsUserspace();

            uiBuilder.Root.OnPrepareDestroy += (slot) => dialogState.Dispose();
            //TODO: move change and error handling to Dialog, make it capable of nested dialogs
            //(makes precomputed boundErrorKeys more difficult)
            //TODO: future-proof dynamically adding/removing dialogs (really?)
            Action<object> onInput = onInputOverride ?? ((object key) =>
            {
                var errors = dialogState.UpdateAndValidate(key);
                var unboundErrors = new Dictionary<object, string>(errors);
                foreach (var errorKey in boundErrorKeys)
                {
                    unboundErrors.Remove(errorKey);
                }

                world.RunSynchronously(() =>
                {
                    foreach (var element in elements)
                    {
                        element.DisplayErrors(errors, unboundErrors);
                    }
                });
            });

            foreach (var definition in definitions)
            {
                var element = definition.Create(uiBuilder, dialogState, onInput, inUserspace);
                if (element != null)
                {
                    elements.Add(element);
                    foreach (var key in element.BoundErrorKeys)
                    {
                        boundErrorKeys.Add(key);
                    }
                }
            }

            var dialog = new Dialog(dialogState, dialogRoot, elements, parent); //executes dialogState.Bind(dialog) already
            //TODO: may be optimized for things like 
            onInput(null);

            uiBuilder.PopStyle();
            return dialog;
        }

        /// <summary>
        /// Creates a dialog window and positions it in front of the user
        /// </summary>
        /// <param name="title">title text of the window</param>
        /// <param name="world">world to place the window in, userspace will directly editing secret options</param>
        /// <param name="dialogState">dialog object</param>
        /// <param name="parent">parent dialog</param>
        /// <returns>(The dialog reference, the root of the created window)</returns>
        public (Dialog dialog, Slot window) BuildWindow(string title, World world, TDialogState dialogState, Dialog parent = null)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (dialogState == null) throw new ArgumentNullException(nameof(dialogState));

            var slot = world.AddSlot(title, persistent: false);
            slot.AttachComponent<NoDestroyUndo>();
            slot.AttachComponent<DuplicateBlock>();

            var uiBuilder = RadiantUI_Panel.SetupPanel(slot, title, ModInstance.Current.MaxCanvasSize);
            var scale = ModInstance.Current.UnitScale;
            slot.GlobalScale = new float3(scale, scale, scale);

            uiBuilder.VerticalLayout(spacing: 0, paddingBottom: 16f, paddingTop: 0f, paddingLeft: 0f, paddingRight: 0f);

            //uiBuilder.Style.ForceExpandWidth = false;
            uiBuilder.Style.ForceExpandHeight = false;

            var scrollRect = uiBuilder.ScrollArea(Alignment.TopLeft);
            //problem: cannot measure size here, solution: extra layer for content
            //new problem: scrolling does not work when using FitContent on second layer only, solution: do it on 1st layer too
            uiBuilder.VerticalLayout(spacing: ModInstance.Current.Spacing);
            uiBuilder.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);

            var content = uiBuilder.VerticalLayout(ModInstance.Current.Spacing, childAlignment: Alignment.TopLeft).Slot;
            uiBuilder.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);

            var dialog = BuildInPlace(uiBuilder, dialogState, parent);

            var offsetFlux = slot.AddSlot("CanvasSizeDriver");
            var contentSizeDriver = content.AttachComponent<RectSizeDriver>();
            contentSizeDriver.Scale.Value = new float2(0, 1);
            var contentSize = offsetFlux.AttachComponent<ValueField<float2>>();
            var offset = offsetFlux.AttachComponent<ValueInput<float2>>();
            offset.Value.Value = ModInstance.Current.CanvasSizeOffset;
            contentSize.Value.Value = uiBuilder.Canvas.Size - offset.Value.Value;
            contentSizeDriver.TargetSize.Target = contentSize.Value;
            var add = offsetFlux.AttachComponent<ValueAdd<float2>>();
            var contentSizeSource = offsetFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueSource<float2>>();
            var contentSizeReference = offsetFlux.AttachComponent<GlobalReference<IValue<float2>>>();
            contentSizeSource.Source.Target = contentSizeReference;
            contentSizeReference.Reference.Target = contentSize.Value;
            add.A.Target = contentSizeSource;
            add.B.Target = offset;
            var min = offsetFlux.AttachComponent<ValueMin<float2>>();
            var maxSize = offsetFlux.AttachComponent<ValueInput<float2>>();
            maxSize.Value.Value = ModInstance.Current.MaxCanvasSize;
            min.A.Target = add;
            min.B.Target = maxSize;
            var canvasSizeDriver = offsetFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueFieldDrive<float2>>();
            canvasSizeDriver.Value.Target = min;
            var canvasSizeProxy = offsetFlux.AttachComponent<FrooxEngine.ProtoFlux.CoreNodes.FieldDriveBase<float2>.Proxy>();
            canvasSizeProxy.Node.Target = canvasSizeDriver;
            canvasSizeProxy.Drive.Target = uiBuilder.Canvas.Size;

            slot.PositionInFrontOfUser(float3.Backward);

            return (dialog, slot);
        }
    }
}