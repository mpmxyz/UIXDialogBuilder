using Elements.Core;

namespace UIXDialogBuilder
{
    public static class DefaultConfigs
    {
        public static string SecretEditorTitle => "Edit...";
        public static string OpenSecretEditorTitle => "Edit";
        public static string SecretEditorAcceptText => "OK";
        public static string SecretPatternText => "*";
        public static float ConfigPanelHeight => 0.25f;
        public static float Spacing => 4f;
        public static float ButtonHeight => 24f;
        public static float ErrorHeight => 8f;
        public static float2 CanvasSize => new float2(200f, 108f);

        public static bool DebugEnabled => true;
    }
}