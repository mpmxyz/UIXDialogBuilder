using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Definition of a text output that displays validation errors.
    /// </summary>
    /// <typeparam name="TDialogState">type of the dialog object</typeparam>
    public class DialogErrorDisplayDefinition<TDialogState> : IDialogEntryDefinition<TDialogState> where TDialogState : IDialogState
    {//TODO: add filter/binding
        private readonly object key;
        private readonly bool onlyUnbound;
        private readonly int nLines;

        /// <summary>
        /// Creates an error display definition
        /// </summary>
        /// <param name="onlyUnbound">true to only display errors that are not already displayed at an dialog option</param>
        /// <param name="nLines">height of the display in lines</param>
        public DialogErrorDisplayDefinition(object key, bool onlyUnbound, int nLines = 2)
        {
            this.key = key;
            this.onlyUnbound = onlyUnbound;
            this.nLines = nLines;
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
            if (onInput == null) throw new ArgumentNullException(nameof(onInput));

            uiBuilder.PushStyle();
            uiBuilder.Style.PreferredHeight = ModInstance.Current.ErrorHeight * nLines;
            uiBuilder.Style.TextColor = colorX.Red;
            Text text = uiBuilder.Text("", alignment: Alignment.MiddleRight);
            uiBuilder.PopStyle();
            return new Element(key, text.Slot, text.Content, onlyUnbound);
        }

        private class Element : DialogElementBase
        {
            private readonly object _Key;
            private readonly Slot _Slot;
            private readonly IValue<string> _Text;
            private readonly bool _OnlyUnbound;

            public Element(object key, Slot slot, IValue<string> text, bool onlyUnbound)
            {
                _Key = key;
                _Slot = slot;
                _Text = text;
                _OnlyUnbound = onlyUnbound;
            }

            public override object Key => _Key;

            public override IEnumerable<object> BoundErrorKeys => new List<object>();

            public override bool Visible
            {
                get => _Slot.ActiveSelf;
                set => _Slot.ActiveSelf = value;
            }
            internal override bool EffectivelyEnabled
            {
                set { }
            }

            public override void DisplayErrors(IDictionary<object, string> allErrors, IDictionary<object, string> unboundErrors)
            {
                IDictionary<object, string> displayedErrors = _OnlyUnbound ? unboundErrors : allErrors;
                _Text.Value = $"<b>{string.Join("\n", displayedErrors.Values)}</b>";
            }

            public override void Reset()
            {

            }
        }
    }
}