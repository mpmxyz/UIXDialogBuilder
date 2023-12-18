using FrooxEngine;
using System.Collections.Generic;
using FrooxEngine.UIX;
using System.Reflection;
using System;
using Elements.Core;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators;
using FrooxEngine.ProtoFlux;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Helper class to create an import dialog <br/>
    /// To be exact this class starts its life as a builder where you can add defaults or custom elements to define a dialog.
    /// Then you can use it as a factory to produce new UIX windows from objects representing your custom dialog.
    /// </summary>
    /// <typeparam name="T">expected dialog object type</typeparam>
    public partial class DialogBuilder<T> where T : IDialogState
    {
        private readonly List<IDialogEntryDefinition<T>> definitions = new List<IDialogEntryDefinition<T>>();
        private readonly Func<T, (IDictionary<object, string>, IDictionary<object, string>)> overrideUpdateAndValidate;

        /// <summary>
        /// Creates a dialog builder that can be configured to create dialog windows.
        /// </summary>
        /// <param name="addDefaults">creates a list of options, an output for errors and a line with buttons based on <typeparamref name="T"/>'s attributes</param>
        /// <param name="overrideUpdateAndValidate">replaces the default validation if not null</param>
        public DialogBuilder(bool addDefaults = true, Func<T, (IDictionary<object, string>, IDictionary<object, string>)> overrideUpdateAndValidate = null)
        {
            if (addDefaults)
            {
                AddAllOptions();
                AddUnboundErrorDisplay();
                AddAllActions();
            }

            this.overrideUpdateAndValidate = overrideUpdateAndValidate;
        }

        /// <summary>
        /// Adds a line to the dialog configuration
        /// </summary>
        /// <param name="optionField">object that will generate UI for an instance of <typeparamref name="T"/></param>
        /// <returns>this</returns>
        public DialogBuilder<T> AddEntry(IDialogEntryDefinition<T> optionField)
        {
            definitions.Add(optionField);
            return this;
        }

        /// <summary>
        /// Adds a line for each of <typeparamref name="T"/>'s attributes annotated with <see cref="DialogOptionAttribute"/>
        /// </summary>
        /// <returns>this</returns>
        public DialogBuilder<T> AddAllOptions()
        {
            var converterType = typeof(T);

            foreach (var fieldInfo in converterType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attr in fieldInfo.GetCustomAttributes(true))
                {
                    if (attr is DialogOptionAttribute conf)
                    {
                        AddOption(conf, fieldInfo);
                        break;
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a line with one button for each of <typeparamref name="T"/>'s attributes annotated with <see cref="DialogActionAttribute"/>
        /// </summary>
        /// <returns>this</returns>
        /// <exception cref="InvalidOperationException">If an annotated method has arguments.</exception>
        public DialogBuilder<T> AddAllActions()
        {
            var converterType = typeof(T);
            var actions = new List<DialogActionDefinition<T>>();
            foreach (var methodInfo in converterType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attr in methodInfo.GetCustomAttributes(true))
                {
                    if (attr is DialogActionAttribute conf)
                    {
                        if (methodInfo.GetParameters().Length != 0)
                        {
                            throw new InvalidOperationException($"DialogAction '{methodInfo.Name}' must have no arguments!");
                        }
                        actions.Add(new DialogActionDefinition<T>(methodInfo.Name, conf, (dialog) => methodInfo.Invoke(dialog, Array.Empty<object>())));
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
        /// <param name="fieldInfo">field of <typeparamref name="T"/> which will be edited</param>
        /// <returns>this</returns>
        public DialogBuilder<T> AddOption(DialogOptionAttribute conf, FieldInfo fieldInfo)
        {
            if (conf == null) throw new ArgumentNullException(nameof(conf));
            if (fieldInfo == null) throw new ArgumentNullException(nameof(fieldInfo));

            var genType = typeof(DialogOptionDefinition<,>).MakeGenericType(typeof(T), fieldInfo.FieldType);
            var cons = genType.GetConstructor(
                    new Type[] {
                        typeof(FieldInfo),
                        typeof(DialogOptionAttribute)
                    }
                );
            var field = (IDialogEntryDefinition<T>)cons.Invoke(new object[] { fieldInfo, conf });
            return AddEntry(field);
        }

        /// <summary>
        /// Adds a line with multiple sub-elements
        /// </summary>
        /// <param name="elements">Elements that will be placed in a single line.</param>
        /// <returns>this</returns>
        public DialogBuilder<T> AddLine(object key, IEnumerable<IDialogEntryDefinition<T>> elements)
        {
            AddEntry(new DialogLineDefinition<T>(key, elements));
            return this;
        }

        /// <summary>
        /// Creates a text output that shows a list of all errors that are not displayed directly on the problematic input.
        /// </summary>
        /// <returns>this</returns>
        public DialogBuilder<T> AddUnboundErrorDisplay()
        {
            AddEntry(new DialogErrorDisplayDefinition<T>(null, onlyUnbound: true));
            return this;
        }

        /// <summary>
        /// Adds the dialog UI to whereever the <paramref name="uiBuilder"/> is currently at
        /// </summary>
        /// <param name="uiBuilder">used to build the UI</param>
        /// <param name="dialog">object that will be configured by the UI</param>
        public void BuildInPlace(UIBuilder uiBuilder, T dialog)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));

            var elements = new List<IDialogElement>();
            var boundErrorKeys = new HashSet<object>();
            var world = uiBuilder.World;
            var inUserspace = world.IsUserspace();

            uiBuilder.Root.OnPrepareDestroy += (slot) => dialog.Dispose();

            (IDictionary<object, string>, IDictionary<object, string>) onChange()
            {
                IDictionary<object, string> errors, unboundErrors;
                if (overrideUpdateAndValidate != null)
                {
                    (errors, unboundErrors) = overrideUpdateAndValidate(dialog);
                }
                else
                {
                    errors = dialog.UpdateAndValidate();
                    unboundErrors = new Dictionary<object, string>(errors);
                    foreach (var errorKey in boundErrorKeys)
                    {
                        unboundErrors.Remove(errorKey);
                    }
                }

                world.RunSynchronously(() =>
                {
                    foreach (var element in elements)
                    {
                        element.DisplayErrors(errors, unboundErrors);
                    }
                });

                return (errors, unboundErrors);
            }

            foreach (var definition in definitions)
            {
                var element = definition.Create(uiBuilder, dialog, onChange, inUserspace);
                if (element != null)
                {
                    elements.Add(element);
                    foreach (var key in element.BoundErrorKeys)
                    {
                        boundErrorKeys.Add(key);
                    }
                }
            }

            onChange();
        }

        /// <summary>
        /// Creates a dialog window and positions it in front of the user
        /// </summary>
        /// <param name="title">title text of the window</param>
        /// <param name="world">world to place the window in, userspace will directly editing secret options</param>
        /// <param name="dialog">dialog object</param>
        /// <returns>The root of the created window</returns>
        public Slot BuildWindow(string title, World world, T dialog)
        {
            if (title == null) throw new ArgumentNullException(nameof(title));
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (dialog == null) throw new ArgumentNullException(nameof(dialog));

            var slot = world.AddSlot(title, persistent: false);
            //TODO: adjust to return dialog instance and window slot (combined in DialogWindow?
            //)
            var uiBuilder = RadiantUI_Panel.SetupPanel(slot, title, ModInstance.Current.CanvasSize);
            var scale = ModInstance.Current.UnitScale;
            slot.GlobalScale = new float3(scale, scale, scale);

            //uiBuilder.Style.ForceExpandWidth = false;
            uiBuilder.Style.ForceExpandHeight = false;
            uiBuilder.ScrollArea();
            uiBuilder.VerticalLayout(ModInstance.Current.Spacing);                      //problem: cannot measure size here
            var content = uiBuilder.VerticalLayout(ModInstance.Current.Spacing).Slot;   //solution: extra layer for content
            
            uiBuilder.FitContent(SizeFit.Disabled, SizeFit.PreferredSize); //TODO: clamp to max-size
            BuildInPlace(uiBuilder, dialog);

            var offsetFlux = slot.AddSlot("CanvasSizeDriver");
            var contentSizeDriver = content.AttachComponent<RectSizeDriver>();
            var contentSize = offsetFlux.AttachComponent<ValueField<float2>>();
            var offset = offsetFlux.AttachComponent<ValueInput<float2>>();
            offset.Value.Value = new float2(32, 120);
            contentSize.Value.Value = uiBuilder.Canvas.Size - offset.Value.Value;
            contentSizeDriver.TargetSize.Target = contentSize.Value;
            var add = offsetFlux.AttachComponent<ValueAdd<float2>>();
            var contentSizeSource = offsetFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueSource<float2>>();
            var contentSizeReference = offsetFlux.AttachComponent<GlobalReference<IValue<float2>>>();
            contentSizeSource.Source.Target = contentSizeReference;
            contentSizeReference.Reference.Target = contentSize.Value;
            add.A.Target = contentSizeSource;
            add.B.Target = offset;
            var canvasSizeDriver = offsetFlux.AttachComponent<FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes.ValueFieldDrive<float2>>();
            canvasSizeDriver.Value.Target = add;
            var canvasSizeProxy = offsetFlux.AttachComponent<FrooxEngine.ProtoFlux.CoreNodes.FieldDriveBase<float2>.Proxy>();
            canvasSizeProxy.Node.Target = canvasSizeDriver;
            canvasSizeProxy.Drive.Target = uiBuilder.Canvas.Size;

            slot.PositionInFrontOfUser(float3.Backward);

            return slot;
        }
    }
}