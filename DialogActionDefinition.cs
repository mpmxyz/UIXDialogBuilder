﻿using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Defines a simple button with an action
    /// </summary>
    /// <typeparam name="TDialogState">type of the expected dialog object</typeparam>
    public class DialogActionDefinition<TDialogState> : IDialogEntryDefinition<TDialogState> where TDialogState : IDialogState
    {
        private readonly object key;
        private readonly DialogActionAttribute conf;

        private readonly Action<TDialogState, User> action;

        /// <summary>
        /// Creates an action definition
        /// </summary>
        /// <param name="key">key to change display/interaction</param>
        /// <param name="conf">displayed text and validation behaviour</param>
        /// <param name="action">Action that is triggered when pressing the button</param>
        public DialogActionDefinition(object key, DialogActionAttribute conf, Action<TDialogState, User> action)
        {
            this.key = key;
            this.conf = conf;
            this.action = action;
        }

        public IDialogElement
            Create(
            UIBuilder uiBuilder,
            TDialogState dialogState,
            Action<object> onInput,
            bool inUserspace = false)
        {
            if (uiBuilder == null) throw new ArgumentNullException(nameof(uiBuilder));
            if (dialogState == null) throw new ArgumentNullException(nameof(dialogState));

            uiBuilder.PushStyle();
            uiBuilder.Style.TextAutoSizeMax = ModInstance.Current.LineHeight;
            uiBuilder.Style.MinHeight = ModInstance.Current.LineHeight;
            uiBuilder.Style.PreferredHeight = ModInstance.Current.LineHeight;
            uiBuilder.Style.FlexibleWidth = 1f;

            Button button = uiBuilder.Button(conf.Name);
            uiBuilder.PopStyle();

            var element = new Element(key, button.Slot, button, conf.OnlyValidating);

            void onPressed(User user)
            {
                if (element._EffectivelyEnabled)
                {
                    //"unnecessary" check to avoid running Validate
                    if (!element.IsValidating || element.IsValid(dialogState.UpdateAndValidate(null)))
                    {
                        action(dialogState, user);
                    }
                }
            }

            if (conf.IsPrivate)
            {
                button.AddPrivateAction(onPressed);
            }
            else
            {
                button.AddPublicAction(onPressed);
            }
            element.UpdateEnabled();

            return element;
        }

        private class Element : DialogElementBase
        {
            private readonly object _Key;
            private readonly Slot _Slot;
            private readonly IButton _Button;
            private readonly object[] _ValidationFilter;
            public bool _EffectivelyEnabled = true;
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
                    _EffectivelyEnabled = value;
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

            internal void UpdateEnabled()
            {
                _Button.Enabled = _EffectivelyEnabled && _IsValid;
            }
        }
    }
}