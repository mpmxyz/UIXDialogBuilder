using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;

namespace UIXDialogBuilder
{
    internal class PatchesHarmony
    {
        internal static void Apply()
        {
            Harmony harmony = new Harmony("com.github.mpmxyz.UIXDialogBuilder");
            harmony.PatchAll();
        }

#pragma warning disable IDE0051 // Remove unused private members
        [HarmonyPatch(typeof(DevTool), "GenerateMenuItems")]
        class ClassName_MethodName_Patch
        {
            [HarmonyPostfix]
            static void AppendContextMenu(InteractionHandler tool, ContextMenu menu)
            {
                if (!ModInstance.Current.DebugEnabled)
                {
                    return; //In this example if the mod is not enabled, we'll just return before doing anything
                }
                var item = menu.AddItem("Test dialog", (Uri)null, colorX.Black);
                item.Button.LocalPressed += (b, e) => new DialogBuilder<TestDialogState>()
                .BuildWindow(
                    "Test",
                    menu.World,
                    new TestDialogState());
            }
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}
