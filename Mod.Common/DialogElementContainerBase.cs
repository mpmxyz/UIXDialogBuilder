using System.Collections.Generic;

namespace UIXDialogBuilder
{
    public abstract class DialogElementContainerBase : DialogElementBase
    {
        internal abstract IEnumerable<IDialogElement> Elements { get; }

        public override IEnumerable<object> BoundErrorKeys
        {
            get
            {
                var allKeys = new HashSet<object>();
                foreach (var element in Elements)
                {
                    foreach (var key in element.BoundErrorKeys)
                    {
                        allKeys.Add(key);
                    }
                }
                return allKeys;
            }
        }

        internal override bool EffectivelyEnabled
        {
            set
            {
                foreach (var element in Elements)
                {
                    element.ParentEnabled = value;
                }
            }
        }

        public override void Reset()
        {
            //noop for *this*
        }

        public override void DisplayErrors(IDictionary<object, string> allErrors, IDictionary<object, string> unboundErrors)
        {
            foreach (var element in Elements)
            {
                element.DisplayErrors(allErrors, unboundErrors);
            }
        }


        public override void ShowAll()
        {
            base.ShowAll();
            foreach (var element in Elements)
            {
                element.ShowAll();
            }
        }
        public override void Show(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.Show(keys);
            foreach (var element in Elements)
            {
                element.Show(keys);
            }
        }

        public override void HideAll()
        {
            base.HideAll();
            foreach (var element in Elements)
            {
                element.HideAll();
            }
        }
        public override void Hide(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.Hide(keys);
            foreach (var element in Elements)
            {
                element.Hide(keys);
            }
        }

        public override void SetVisible(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.SetVisible(keys);
            foreach (var element in Elements)
            {
                element.SetVisible(keys);
            }
        }

        public override void EnableAll()
        {
            base.EnableAll();
            foreach (var element in Elements)
            {
                element.EnableAll();
            }
        }
        public override void Enable(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.Enable(keys);
            foreach (var element in Elements)
            {
                element.Enable(keys);
            }
        }

        public override void DisableAll()
        {
            base.DisableAll();
            foreach (var element in Elements)
            {
                element.DisableAll();
            }
        }
        public override void Disable(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.Disable(keys);
            foreach (var element in Elements)
            {
                element.Disable(keys);
            }
        }

        public override void SetEnabled(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.SetEnabled(keys);
            foreach (var element in Elements)
            {
                element.SetEnabled(keys);
            }
        }

        public override void ResetAll()
        {
            base.ResetAll();
            foreach (var element in Elements)
            {
                element.ResetAll();
            }
        }

        public override void Reset(IEnumerable<object> keys)
        {
            keys = OptimizeLookup(keys);
            base.Reset(keys);
            foreach (var element in Elements)
            {
                element.Reset(keys);
            }
        }

        private static ISet<object> OptimizeLookup(IEnumerable<object> keys)
        {
            return keys as ISet<object> ?? new HashSet<object>(keys);
        }
    }
}
