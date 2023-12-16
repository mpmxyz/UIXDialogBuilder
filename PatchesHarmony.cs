using FrooxEngine.UIX;
using HarmonyLib;

namespace UIXDialogBuilder
{
    internal class PatchesHarmony
    {
        private static IUIXDialogBuilder ModInstance;

        internal static void Apply(IUIXDialogBuilder instance)
        {
            ModInstance = instance;
            Harmony harmony = new Harmony("com.github.mpmxyz.UIXDialogBuilder"); //typically a reverse domain name is used here (https://en.wikipedia.org/wiki/Reverse_domain_name_notation)
            harmony.PatchAll(); // do whatever LibHarmony patching you need, this will patch all [HarmonyPatch()] instances
        }
        //Example of how a HarmonyPatch can be formatted
        [HarmonyPatch(typeof(Button), "OnPressBegin")]
        class ClassName_MethodName_Patch
        {
            //Postfix() here will be automatically applied as a PostFix Patch
            [HarmonyPostfix]
            static void Postfix(Button __instance, Canvas.InteractionData eventData)
            {
                if (!ModInstance.Enabled)
                {//Use Config.GetValue() to use the ModConfigurationKey defined earlier
                    return; //In this example if the mod is not enabled, we'll just return before doing anything
                }
                ModInstance.DoSomething();
                FrooxEngineBootstrap.LogStream.Flush();
                //Do stuff after everything in the original OnPressBegin has run.
            }
        }
    }
}
