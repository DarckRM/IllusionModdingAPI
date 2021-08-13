﻿using System;
using System.Collections;
using ActionGame;
using HarmonyLib;

namespace KKAPI.MainGame
{
    public static partial class GameAPI
    {
        private class Hooks
        {
            public static void SetupHooks(Harmony hi)
            {
                hi.PatchAll(typeof(Hooks));
                //Patch the VR version of these methods via reflection since they don't exist in normal assembly
                var vrHSceneType = Type.GetType("VRHScene, Assembly-CSharp");
                if (vrHSceneType != null)
                {
                    hi.Patch(AccessTools.Method(vrHSceneType, "Start"), new HarmonyMethod(AccessTools.Method(typeof(Hooks), nameof(Hooks.StartProcPost))));
                    hi.Patch(AccessTools.Method(vrHSceneType, "EndProc"), new HarmonyMethod(AccessTools.Method(typeof(Hooks), nameof(Hooks.EndProcPost))));
                    hi.Patch(AccessTools.Method(vrHSceneType, "OnBack"), new HarmonyMethod(AccessTools.Method(typeof(Hooks), nameof(Hooks.EndProcPost))));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Load), new[] { typeof(string), typeof(string) })]
            public static void LoadHook(string path, string fileName)
            {
                OnGameBeingLoaded(path, fileName);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(SaveData), nameof(SaveData.Save), new[] { typeof(string), typeof(string) })]
            public static void SaveHook(string path, string fileName)
            {
                OnGameBeingSaved(path, fileName);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "Start")]
            public static void StartProcPost(HSceneProc __instance, ref IEnumerator __result)
            {
                var oldResult = __result;
                __result = new[] { oldResult, OnHStart(__instance) }.GetEnumerator();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "NewHeroineEndProc")]
            public static void NewHeroineEndProcPost(HSceneProc __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), "EndProc")]
            public static void EndProcPost(HSceneProc __instance)
            {
                OnHEnd(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[] { typeof(Cycle.Type) })]
            public static void CycleChangeTypeHook(Cycle.Type type)
            {
                OnPeriodChange(type);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Cycle), nameof(Cycle.Change), new Type[] { typeof(Cycle.Week) })]
            public static void CycleChangeWeekHook(Cycle.Week week)
            {
                OnDayChange(week);
            }
        }
    }
}
