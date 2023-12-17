using FrooxEngine;
using FrooxEngine.UIX;
using System.Reflection;
using System;
using System.Collections.Generic;
using Elements.Core;
using System.Xml.Linq;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Defines a configuration option in the dialog window
    /// </summary>
    /// <typeparam name="TDialogState">type of the dialog object</typeparam>
    /// <typeparam name="TValue">type of the edited value</typeparam>
    public class DialogOptionDefinition<TDialogState, TValue> : IDialogEntryDefinition<TDialogState> where TDialogState : IDialogState
    {
        private readonly DialogOptionAttribute conf;

        private readonly FieldInfo fieldInfo;

        /// <summary>
        /// Creates a configuration option
        /// </summary>
        /// <param name="conf">displayed name, secrecy and error output options</param>
        /// <param name="fieldInfo">field of type <typeparamref name="TDialogState"/> which will be edited</param>
        public DialogOptionDefinition(DialogOptionAttribute conf, FieldInfo fieldInfo)
        {
            this.conf = conf;
            this.fieldInfo = fieldInfo;
        }

        public IDialogElement
            Create(UIBuilder uiBuilder, TDialogState dialogState, Func<(IDictionary<object, string>, IDictionary<object, string>)> onChange, bool inUserspace = false)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (dialogState == null) throw new ArgumentNullException(nameof(dialogState));
            if (onChange == null) throw new ArgumentNullException(nameof(onChange));

            var slot = uiBuilder.VerticalLayout(spacing: ModInstance.Current.Spacing / 2).Slot;
            IValue<string> errorTextContent;
            Action reset = StaticBuildFunctions.BuildLineWithLabel(conf.Name, uiBuilder, (uiBuilder2) =>
            {
                if (conf.Secret && !inUserspace)
                {
                    //TODO: ensure proper reset functionality
                    var secretDialog = new SecretDialog(this, dialogState, onChange);
                    StaticBuildFunctions.BuildSecretButton(uiBuilder2, () => secretDialog.Open());
                    return secretDialog.Reset;
                }
                else
                {
                    return StaticBuildFunctions.BuildEditor(uiBuilder2.Root, dialogState, fieldInfo, () => onChange(), uiBuilder2, conf);
                }
            });
            
            var key = fieldInfo.Name;
            if (conf.ShowErrors)
            {
                uiBuilder.PushStyle();
                uiBuilder.Style.PreferredHeight = ModInstance.Current.ErrorHeight;
                uiBuilder.Style.TextColor = colorX.Red;
                var errorText = uiBuilder.Text("", alignment: Alignment.TopRight);
                uiBuilder.PopStyle();
                uiBuilder.NestOut();
                errorTextContent = errorText.Content;
            }
            else
            {
                errorTextContent = null;
            }
            return new Element(key, slot, errorTextContent, reset);
        }

        private class Element : DialogElementBase
        {
            private readonly object _Key;
            private readonly Slot _Slot;
            private readonly IValue<string> _ErrorField;
            private readonly Action _Reset;

            public Element(object key, Slot slot, IValue<string> errorField, Action reset)
            {
                _Key = key;
                _Slot = slot;
                _ErrorField = errorField;
                _Reset = reset;
            }

            public override object Key => _Key;

            public override IEnumerable<object> BoundErrorKeys => new List<object>(new object[] { _Key });

            public override bool Visible
            { 
                get => _Slot.ActiveSelf; 
                set => _Slot.ActiveSelf = value;
            }
            internal override bool EffectivelyEnabled
            {
                set => _Slot.GetComponentsInChildren<InteractionElement>().ForEach(it => it.Enabled = value);
            }

            public override void DisplayErrors(IDictionary<object, string> allErrors, IDictionary<object, string> unboundErrors)
            {
                if (_ErrorField != null)
                {
                    _ErrorField.Value = allErrors.TryGetValue(_Key, out var error)
                                    ? $"<b>{error}</b>"
                                    : "";
                }
            }

            public override void Reset()
            {
                _Reset();
            }
        }

        /// <summary>
        /// represents a dialog that edits a single value in userspace
        /// </summary>
        private class SecretDialog
        {
            private readonly DialogBuilder<TDialogState> dialogBuilder;
            private readonly string title;
            private readonly TDialogState dialog;
            private Slot slot;

            public SecretDialog(DialogOptionDefinition<TDialogState, TValue> option, TDialogState dialog, Func<(IDictionary<object, string>, IDictionary<object, string>)> onChangeSource)
            {
                //TODO: Dialog binding to IDialogState must be adjusted to cater for popups like this (potential target: condition/config in builder)
                this.dialogBuilder = new DialogBuilder<TDialogState>(addDefaults: false, overrideUpdateAndValidate: (_) => onChangeSource())
                        .AddEntry(option)
                        .AddEntry(new DialogActionDefinition<TDialogState>(
                            null,
                            new DialogActionAttribute(ModInstance.Current.SecretEditorAcceptText, onlyValidating: Array.Empty<object>()),
                            (x) => Close()
                            ));
                this.title = option.conf.Name;
                this.dialog = dialog;
            }

            public void Open()
            {
                Userspace.UserspaceWorld.RunSynchronously(() =>
                {
                    slot?.Destroy();
                    slot = dialogBuilder.BuildWindow(title, Userspace.UserspaceWorld, dialog);
                    var editor = slot.GetComponentInChildren<TextEditor>();
                    editor?.Focus();
                });
            }

            public void Close()
            {
                Userspace.UserspaceWorld.RunSynchronously(() =>
                {
                    slot?.Destroy();
                    slot = null;
                });
            }

            public void Reset()
            {
                UniLog.Warning("TODO: Reset " + this);
            }
        }
    }
}