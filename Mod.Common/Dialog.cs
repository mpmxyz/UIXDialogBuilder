using FrooxEngine;
using System;
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
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            _Slot = root;
            _Elements = elements;
            _Parent = parent;
            if (parent == null)
            {
                if (state.Dialog != null) throw new ArgumentException("Dialog state is already bound to a dialog!", nameof(state));
                state.Dialog = this;
                //automatically unbind when dialog UI is destroyed:
                root.OnPrepareDestroy += (slot) =>
                {
                    if (state.Dialog == this)
                    {
                        state.Dispose();
                        state.Dialog = null;
                    }
                };
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
