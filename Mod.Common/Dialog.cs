using FrooxEngine;
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    public class Dialog : DialogElementContainerBase
    {
        private readonly Slot _Slot;
        private readonly IEnumerable<IDialogElement> _Elements;
        private readonly Dialog _Parent;

        internal Dialog(IDialogState state, Slot root, IEnumerable<IDialogElement> elements, Dialog parent = null)
        {
            _Slot = root;
            _Elements = elements;
            _Parent = parent;
            if (parent == null)
            {
                state.Dialog = this;
            }
        }

        public Dialog RootDialog => _Parent?.RootDialog ?? this;
        public Slot Slot => _Slot;

        public override object Key => this;

        public override bool Visible
        {
            get => _Slot.ActiveSelf;
            set => _Slot.ActiveSelf = value;
        }

        internal override IEnumerable<IDialogElement> Elements => _Elements;
    }
}
