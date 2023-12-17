using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Defines a simple button with an action
    /// </summary>
    /// <typeparam name="T">type of the expected dialog object</typeparam>
    public class DialogActionDefinition<T> : IDialogEntryDefinition<T> where T : IDialogState
    {
        private readonly object key;
        private readonly DialogActionAttribute conf;

        private readonly Action<T> action;

        /// <summary>
        /// Creates an action definition
        /// </summary>
        /// <param name="key">key to change display/interaction</param>
        /// <param name="conf">displayed text and validation behaviour</param>
        /// <param name="action">Action that is triggered when pressing the button</param>
        public DialogActionDefinition(object key, DialogActionAttribute conf, Action<T> action)
        {
            this.key = key;
            this.conf = conf;
            this.action = action;
        }

        public IDialogElement
            Create(UIBuilder uiBuilder, T dialogState, Func<(IDictionary<object, string>, IDictionary<object, string>)> onChange, bool inUserspace = false)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (dialogState == null) throw new ArgumentNullException(nameof(dialogState));
            //TODO: investigate what onChange is supposed to do

            uiBuilder.PushStyle();
            uiBuilder.Style.PreferredHeight = ModInstance.Current.ButtonHeight;
            Button button = uiBuilder.Button(conf.Name);
            uiBuilder.PopStyle();

            var element = new Element(key, button.Slot, button, conf.OnlyValidating);
            button.LocalPressed += (IButton b, ButtonEventData bed) =>
            {
                //"unnecessary" conf check to avoid running Validate
                if (!element.IsValidating || element.IsValid(dialogState.UpdateAndValidate()))
                {
                    action(dialogState);
                }
            };
            return element;
        }

        private class Element : DialogElementBase
        {
            private readonly object _Key;
            private readonly Slot _Slot;
            private readonly IButton _Button;
            private readonly object[] _ValidationFilter;
            private bool _Enabled = true;
            private bool _IsValid = true;

            public Element(object key, Slot slot, IButton button, object[] validationFilter)
            {
                _Key = key;
                _Slot = slot;
                _Button = button;
                _ValidationFilter = validationFilter;
            }

            internal bool IsValidating => _ValidationFilter != null && _ValidationFilter.Length > 0;

            public override object Key => _Key;

            public override IEnumerable<object> BoundErrorKeys => new List<object>();

            public override bool Visible
            {
                get => _Slot.ActiveSelf;
                set => _Slot.ActiveSelf = value;
            }

            internal override bool EffectivelyEnabled
            {
                set
                {
                    _Enabled = value;
                    UpdateEnabled();
                }
            }

            public override void DisplayErrors(IDictionary<object, string> allErrors, IDictionary<object, string> unboundErrors)
            {
                _IsValid = IsValid(allErrors);
                UpdateEnabled();
            }

            public override void Reset()
            {

            }

            internal bool IsValid(IDictionary<object, string> errors)
            {
                if (_ValidationFilter == null)
                {
                    return !errors.Any();
                }
                else
                {
                    return !_ValidationFilter.Any(errors.ContainsKey);
                }
            }

            private void UpdateEnabled()
            {
                _Button.Enabled = _Enabled && _IsValid;
            }
        }
    }
}