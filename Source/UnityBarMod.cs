// Copyright 2021 Máté Szabó
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using HarmonyLib;
using RimWorld;
using Verse;

namespace UnityBar
{
    [StaticConstructorOnStartup]
    public static class UnityBarMod
    {
        static UnityBarMod()
        {
            Harmony harmony = new Harmony("mszabo.unitybar");
            harmony.PatchAll();
        }

        /// <summary>
        /// Attach the custom colonist bar as a replacement of the vanilla one.
        /// </summary>
        [HarmonyPatch(typeof(ColonistBar), nameof(ColonistBar.ColonistBarOnGUI))]
        public static class ColonistBar_ColonistBarOnGUI_Patch
        {
            private static bool wasUnityBarMounted = false;

            public static bool Prefix()
            {
                if (!wasUnityBarMounted)
                {
                    Find.Root.gameObject.AddComponent<UnityBar>();
                    wasUnityBarMounted = true;
                }

                return false;
            }
        }

        /// <summary>
        /// Trigger a refresh of colonist bar items when needed.
        /// </summary>
        [HarmonyPatch(typeof(ColonistBar), nameof(ColonistBar.MarkColonistsDirty))]
        public static class ColonistBar_MarkColonistsDirty_Patch
        {
            public static void Postfix() => Find.Root.gameObject.GetComponent<UnityBar>()?.MarkDirty();
        }

        /// <summary>
        /// Update the cached portrait for a given pawn within the colonist bar.
        /// </summary>
        [HarmonyPatch(typeof(PortraitsCache), nameof(PortraitsCache.SetDirty))]
        public static class PortraitsCache_SetDirty_Patch
        {
            public static void Postfix(Pawn pawn) => Find.Root.gameObject.GetComponent<UnityBar>()?.UpdatePortrait(pawn);
        }
    }
}