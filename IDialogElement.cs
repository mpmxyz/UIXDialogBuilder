
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    public interface IDialogElement
    {
        object Key { get; }
        IEnumerable<object> BoundErrorKeys { get; }

        bool ParentEnabled { get; set; }
        bool Enabled { get; set; }
        bool Visible { get; set; }

        /*setting World state to dialog state*/
        void Reset(); //TODO: redesign to allow partial resets without O(n²)
        void DisplayErrors(IDictionary<object, string> allErrors, IDictionary<object, string> unboundErrors);

        void ShowAll();
        void Show(IEnumerable<object> keys);

        void HideAll();
        void Hide(IEnumerable<object> keys);

        void SetVisible(IEnumerable<object> keys);

        void EnableAll();
        void Enable(IEnumerable<object> keys);

        void DisableAll();
        void Disable(IEnumerable<object> keys);

        void SetEnabled(IEnumerable<object> keys);

        void ResetAll();
        void Reset(IEnumerable<object> keys);
    }
}
