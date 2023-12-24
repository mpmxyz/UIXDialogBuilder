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
        float Spacing { get; }
        float LineHeight { get; }
        float ErrorHeight { get; }
        float UnitScale { get; }
        float2 MinCanvasSize { get; }
        float2 MaxCanvasSize { get; }
        float2 CanvasSizeOffset { get; }
    }
}
