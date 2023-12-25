using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Reflection;

namespace UIXDialogBuilder
{
    public interface IEditorGenerator<TValue>
    {
        Action Generate(
            UIBuilder uiBuilder,
            Slot iFieldSlot,
            Action<TValue> setInner,
            Func<TValue> getInner,
            bool isSecret,
            string name,
            ICustomAttributeProvider customAttributes);
    }
}