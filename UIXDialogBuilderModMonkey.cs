
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;
using System;

namespace UIXDialogBuilder
{
    public class UIXDialogBuilderModMonkey : ResoniteMonkey<UIXDialogBuilderModMonkey>, IUIXDialogBuilderMod
    {
        public override string Name => "UIXDialogBuilder";

        private UIXDialogBuilderMonkeyConfig LoadedConfig;

        public bool Enabled => LoadedConfig.Enabled.GetValue();

        public void SpawnSampleDialog()
        {
            Logger.Warn(() => "Hello World!");
        }

        protected override bool OnEngineReady()
        {
            LoadedConfig = Config.LoadSection<UIXDialogBuilderMonkeyConfig>();
            PatchesHarmony.Apply(this);
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
            public DefiningConfigKey<bool> Enabled = new DefiningConfigKey<bool>("Enabled", "Enables a small message on each button click.", () => true);

            public override string Description => "MonkeyLoader flavor of sample mod's config";

            public override string Name => "UIXDialogBuilder";

            public override Version Version => new Version(1, 0, 0);
        }
    }
}
