using FrooxEngine.UIX;
using HarmonyLib;

namespace UIXDialogBuilder
{
    internal class PatchesHarmony
    {
        private static IUIXDialogBuilderMod ModInstance;

        internal static void Apply(IUIXDialogBuilderMod instance)
        {
            ModInstance = instance;
            Harmony harmony = new Harmony("com.github.mpmxyz.UIXDialogBuilder");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Button), "OnPressBegin")]
        class ClassName_MethodName_Patch
        {
            [HarmonyPostfix]
            static void Postfix(Button __instance, Canvas.InteractionData eventData)
            {
                if (!ModInstance.Enabled)
                {
                    return; //In this example if the mod is not enabled, we'll just return before doing anything
                }
                ModInstance.SpawnSampleDialog();
            }
        }
    }
}
