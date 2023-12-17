using System;
using System.Collections.Generic;

namespace UIXDialogBuilder
{

    /// <summary>
    /// Defines the behaviour of a dialog <br/>
    /// <see cref="IDisposable.Dispose"/> is called when the dialog is destroyed (e.g. via the X button)
    /// </summary>
    public interface IDialogState : IDisposable
    {
        /// <summary>
        /// The dialog this state has been bound to (only required to be assignable once)
        /// </summary>
        void Bind(Dialog dialog);

        /// <summary>
        /// Updates internal state and checks for errors
        /// </summary>
        /// <returns>a mapping from field name to the associated error,
        /// disables the validated buttons if non-empty</returns>
        IDictionary<object, string> UpdateAndValidate();
    }
}
