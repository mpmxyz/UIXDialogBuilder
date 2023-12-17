using FrooxEngine;
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    public class Dialog : DialogElementContainerBase
    {
        private readonly Slot _Slot;
        private readonly IEnumerable<IDialogElement> _Elements;

        internal Dialog(IDialogState state, Slot slot, IEnumerable<IDialogElement> elements)
        {
            _Slot = slot;
            _Elements = elements;
            state.Bind(this);
        }

        public override object Key => this;

        public override bool Visible
        {
            get => _Slot.ActiveSelf;
            set => _Slot.ActiveSelf = value;
        }

        internal override IEnumerable<IDialogElement> Elements => _Elements;
    }
}
