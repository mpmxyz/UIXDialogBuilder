using Elements.Core;
using ResoniteModLoader;

namespace UIXDialogBuilder
{
    /// <summary>
    /// This mod is an implementation based on the example given in https://github.com/resonite-modding-group/ResoniteModLoader/blob/main/doc/making_mods.md.
    /// </summary>
    public class UIXDialogBuilderModRML : ResoniteMod, IUIXDialogBuilderMod
    {
        public override string Name => "UIXDialogBuilder";
        public override string Author => "mpmxyz";
        public override string Version => "0.2.0"; //Version of the mod, should match the AssemblyVersion
        public override string Link => "https://github.com/mpmxyz/UIXDialogBuilder";

        public UIXDialogBuilderModRML()
        {
            ModInstance.Current = this;
        }

        //The following
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => DefaultConfigs.DebugEnabled); //Optional config settings

        private static ModConfiguration Config;//If you use config settings, this will be where you interface with them

        public bool DebugEnabled => Config.GetValue(enabled);

        public string SecretEditorTitle => DefaultConfigs.SecretEditorTitle;
        public string OpenSecretEditorTitle => DefaultConfigs.OpenSecretEditorTitle;
        public string SecretEditorAcceptText => DefaultConfigs.SecretEditorAcceptText;
        public string SecretPatternText => DefaultConfigs.SecretPatternText;
        public float Spacing => DefaultConfigs.Spacing;
        public float LineHeight => DefaultConfigs.LineHeight;
        public float ErrorHeight => DefaultConfigs.ErrorHeight;
        public float UnitScale => DefaultConfigs.UnitScale;
        public float2 MinCanvasSize => DefaultConfigs.MinCanvasSize;
        public float2 MaxCanvasSize => DefaultConfigs.MaxCanvasSize;
        public float2 CanvasSizeOffset => DefaultConfigs.CanvasSizeOffset;

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);

            PatchesHarmony.Apply();
        }
    }
}
