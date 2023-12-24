using System;

namespace UIXDialogBuilder
{
    /// <summary>
    /// Attributes methods representing a dialog action (no arguments)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DialogActionAttribute : Attribute
    {
        public string Name { get; }
        public bool IsPrivate { get; }
        public object[] OnlyValidating { get; }


        /// <summary>
        /// Creates an action button in the dialog
        /// </summary>
        /// <param name="name">Display name of the action</param>
        /// <param name="isPrivate">if true, action is only available to creator of dialog</param>
        /// <param name="onlyValidating">explicitly selects keys to validate for</param>
        public DialogActionAttribute(string name, bool isPrivate = true, object[] onlyValidating = null)
        {
            Name = name;
            IsPrivate = isPrivate;
            OnlyValidating = onlyValidating;
        }
    }
}
