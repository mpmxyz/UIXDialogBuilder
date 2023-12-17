using FrooxEngine;
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    public class DialogElementContainer : DialogElementContainerBase
    {
        private readonly object _Key;
        private readonly Slot _Slot;
        private readonly IEnumerable<IDialogElement> _Elements;

        public DialogElementContainer(object key, Slot slot, IEnumerable<IDialogElement> elements)
        {
            _Key = key;
            _Slot = slot;
            _Elements = elements;
        }

        public override object Key => _Key;

        public override bool Visible
        {
            get => _Slot.ActiveSelf;
            set => _Slot.ActiveSelf = value;
        }

        internal override IEnumerable<IDialogElement> Elements => _Elements;
    }
}