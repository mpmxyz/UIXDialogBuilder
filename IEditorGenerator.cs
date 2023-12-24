using FrooxEngine;
using FrooxEngine.UIX;
using System.Reflection;

namespace UIXDialogBuilder
{
    public interface IEditorGenerator<TValue>
    {
        void Generate(UIBuilder uiBuilder, IField field, ICustomAttributeProvider customAttributes);
    }
}