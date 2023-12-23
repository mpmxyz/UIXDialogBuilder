using Elements.Core;

namespace UIXDialogBuilder
{
    public static class DefaultConfigs
    {
        public static string SecretEditorTitle => "Edit...";
        public static string OpenSecretEditorTitle => "Edit";
        public static string SecretEditorAcceptText => "OK";
        public static string SecretPatternText => "*";
        public static float Spacing => 8f;
        public static float LineHeight => 24f;
        public static float ErrorHeight => 12f;
        public static float UnitScale => 1 / 1024f;
        public static float2 MinCanvasSize => new float2(500, 250);
        public static float2 MaxCanvasSize => new float2(500, 800);
        public static float2 CanvasSizeOffset => new float2(500, 131);

        public static bool DebugEnabled => true;
    }
}