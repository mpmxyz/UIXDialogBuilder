
using Elements.Core;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;
using System;

namespace UIXDialogBuilder
{
    public class UIXDialogBuilderModMonkey : ResoniteMonkey<UIXDialogBuilderModMonkey>, IUIXDialogBuilderMod
    {
        public override string Name => "UIXDialogBuilder";

        private UIXDialogBuilderMonkeyConfig LoadedConfig;

        public bool DebugEnabled => LoadedConfig.DebugEnabled.GetValue();

        public string SecretEditorTitle => DefaultConfigs.SecretEditorTitle;
        public string OpenSecretEditorTitle => DefaultConfigs.OpenSecretEditorTitle;
        public string SecretEditorAcceptText => DefaultConfigs.SecretEditorAcceptText;
        public string SecretPatternText => DefaultConfigs.SecretPatternText;
        public float ConfigPanelHeight => DefaultConfigs.ConfigPanelHeight;
        public float Spacing => DefaultConfigs.Spacing;
        public float ButtonHeight => DefaultConfigs.ButtonHeight;
        public float ErrorHeight => DefaultConfigs.ErrorHeight;
        public float2 CanvasSize => DefaultConfigs.CanvasSize;


        public UIXDialogBuilderModMonkey()
        {
            ModInstance.Current = this;
        }

        protected override bool OnEngineReady()
        {
            LoadedConfig = Config.LoadSection<UIXDialogBuilderMonkeyConfig>();
            PatchesHarmony.Apply();
            return base.OnEngineReady();
        }

        protected override void OnEngineShutdownRequested(string reason)
        {
            base.OnEngineShutdownRequested(reason);
        }

        protected override bool OnLoaded()
        {
            return base.OnLoaded();
        }

        protected override bool OnShutdown()
        {
            return base.OnShutdown();
        }

        private class UIXDialogBuilderMonkeyConfig : ConfigSection
        {
            public DefiningConfigKey<bool> DebugEnabled = new DefiningConfigKey<bool>("DebugEnabled", "Enables the option to spawn a test dialog with the DevToolTip context menu.", () => true);

            public override string Description => "MonkeyLoader flavor of sample mod's config";

            public override string Name => "UIXDialogBuilder";

            public override Version Version => new Version(1, 0, 0);
        }
    }
}
