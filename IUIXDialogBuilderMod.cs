using Elements.Core;

namespace UIXDialogBuilder
{
    public interface IUIXDialogBuilderMod
    {
        bool DebugEnabled { get; }

        string SecretEditorTitle { get; }
        string OpenSecretEditorTitle { get; }
        string SecretEditorAcceptText { get; }
        string SecretPatternText { get; }
        float ConfigPanelHeight { get; }
        float Spacing { get; }
        float ButtonHeight { get; }
        float ErrorHeight { get; }
        float2 CanvasSize { get; }
    }
}
