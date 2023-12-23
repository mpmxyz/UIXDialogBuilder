using FrooxEngine.UIX;
using System;
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Defines a line of horizontally arranged sub-elements
    /// </summary>
    /// <typeparam name="TDialogState">type of the dialog object</typeparam>
    public class DialogLineDefinition<TDialogState> : IDialogEntryDefinition<TDialogState> where TDialogState : IDialogState
    {
        private readonly IEnumerable<IDialogEntryDefinition<TDialogState>> _Elements;
        private readonly object _Key;

        /// <summary>
        /// Creates a line of sub-elements
        /// </summary>
        /// <param name="key"></param>
        /// <param name="elements"></param>
        public DialogLineDefinition(object key, IEnumerable<IDialogEntryDefinition<TDialogState>> elements)
        {
            _Key = key;
            _Elements = elements;
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

            var allInstances = new List<IDialogElement>();

            uiBuilder.PushStyle();
            var slot = uiBuilder.HorizontalLayout(spacing: ModInstance.Current.Spacing).Slot;
            uiBuilder.Style.FlexibleWidth = 1;
            uiBuilder.Style.ForceExpandWidth = true;


            foreach (var entry in _Elements)
            {
                var instance = entry.Create(uiBuilder, dialogState, onInput, inUserspace);
                if (instance != null)
                {
                    allInstances.Add(instance);
                }
            }

            uiBuilder.NestOut();
            uiBuilder.PopStyle();
            return new DialogElementContainer(_Key, slot, allInstances);
        }
    }
}