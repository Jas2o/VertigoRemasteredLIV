using HarmonyLib;
using MelonLoader;

namespace VertigoRemasteredLIV {
    [HarmonyPatch]
    public static class Patches {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Vertigo2.Player.VertigoPlayer), "Init")]
        private static void SetUpLiv(Vertigo2.Player.VertigoPlayer __instance) {
            MelonLogger.Msg("## Patches : Player-Init-SetUpLiv ##");
            VertigoRemasteredLIVMod.OnPlayerReady();
        }
    }
}
