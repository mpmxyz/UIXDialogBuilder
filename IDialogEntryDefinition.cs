using FrooxEngine.UIX;
using System;
using System.Collections.Generic;

namespace UIXDialogBuilder
{
    /// <summary>
    /// An object specifying a dialog entry
    /// </summary>
    /// <typeparam name="T">type of the dialog object</typeparam>
    public interface IDialogEntryDefinition<in T> where T : IDialogState
    {
        /// <summary>
        /// Creates a dialog entry
        /// </summary>
        /// <param name="uiBuilder">ui builder to build entry with; on method exit it has to have the same nesting level as during method call</param>
        /// <param name="dialogState">object this dialog is based on, may be used to target getters/setters/methods</param>
        /// <param name="onChange">can be triggered by the ui to signal reevaluation of validity</param>
        /// <param name="inUserspace">signals if dialog is created in userspace</param>
        /// <returns>an element that can be used to control the UI or <see langword="null"/> if no control is intended</returns>
        IDialogElement Create
        (
            UIBuilder uiBuilder,
            T dialogState,
            Action<object> onInput,
            bool inUserspace = false
        );
    }
}