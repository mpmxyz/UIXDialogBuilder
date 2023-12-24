using System.Collections.Generic;
using System.Linq;

namespace UIXDialogBuilder
{
    public abstract class DialogElementBase : IDialogElement
    {
        private bool _ParentEnabled = true;
        private bool _Enabled = true;

        public bool ParentEnabled
        {
            get => _ParentEnabled;
            set
            {
                _ParentEnabled = value;
                EffectivelyEnabled = value & _Enabled;
            }
        }
        public bool Enabled
        {
            get => _Enabled;
            set
            {
                _Enabled = value;
                EffectivelyEnabled = value & _ParentEnabled;
            }
        }

        public abstract object Key { get; }
        public abstract IEnumerable<object> BoundErrorKeys { get; }

        public abstract bool Visible { get; set; }
        internal abstract bool EffectivelyEnabled { set; }

        public abstract void Reset();
        public abstract void DisplayErrors(IDictionary<object, string> allErrors, IDictionary<object, string> unboundErrors);

        public virtual void EnableAll()
        {
            Enabled = true;
        }
        public virtual void Enable(IEnumerable<object> keys)
        {
            if (keys.Contains(Key))
            {
                Enabled = true;
            }
        }

        public virtual void DisableAll()
        {
            Enabled = false;
        }
        public virtual void Disable(IEnumerable<object> keys)
        {
            if (keys.Contains(Key))
            {
                Enabled = false;
            }
        }

        public virtual void SetEnabled(IEnumerable<object> keys)
        {
            Enabled = keys.Contains(Key);
        }
        public virtual void ShowAll()
        {
            Visible = true;
        }

        public virtual void Show(IEnumerable<object> keys)
        {
            if (keys.Contains(Key))
            {
                Visible = true;
            }
        }

        public virtual void HideAll()
        {
            Visible = false;
        }
        public virtual void Hide(IEnumerable<object> keys)
        {
            if (keys.Contains(Key))
            {
                Visible = false;
            }
        }

        public virtual void SetVisible(IEnumerable<object> keys)
        {
            Visible = keys.Contains(Key);
        }

        public virtual void ResetAll()
        {
            Reset();
        }
        public virtual void Reset(IEnumerable<object> keys)
        {
            if (keys.Contains(Key))
            {
                Reset();
            }
        }
    }
}
