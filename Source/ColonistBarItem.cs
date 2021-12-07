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

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using Verse.AI;
using Object = UnityEngine.Object;
using Text = UnityEngine.UI.Text;

namespace UnityBar
{
    /// <summary>
    /// Manage rendering for a single colonist bar item.
    /// </summary>
    [HotSwappable]
    public class ColonistBarItem
    {
        public const int ItemSpacing = 24;
        private const float MoodBarOpacity = 0.44f;

        private static readonly Color NormalMoodColor = new Color(0.4f, 0.47f, 0.53f, MoodBarOpacity);
        private static readonly Color MinorBreakColor = new Color(1f, 47f / 51f, 4f / 255f, MoodBarOpacity);

        private readonly GameObject canvasObject;

        private readonly GameObject pawnBackgroundObject;
        private readonly GameObject moodBarObject;
        private readonly GameObject pawnPortraitObject;
        private readonly GameObject pawnNameObject;
        private readonly GameObject pawnDeadObject;

        private readonly ColonistHighlighter colonistHighlighter;

        private readonly RawImage pawnBackground;
        private readonly RawImage moodBar;
        private readonly RawImage pawnPortrait;
        private readonly RawImage pawnDead;
        private readonly Text pawnName;

        private readonly List<ColonistBarItemIcon> iconsShown = new List<ColonistBarItemIcon>();

        private int? index;
        private int? row;
        private float? scaleFactor;
        private float? mood;
        private string name;
        private int? pawnId;
        private bool? dead;
        private Vector2 offset;

        public ColonistBarItem(GameObject canvasObject)
        {
            this.canvasObject = canvasObject;

            this.pawnBackgroundObject = new GameObject("PawnBG");
            this.moodBarObject = new GameObject("MoodBar");
            this.pawnPortraitObject = new GameObject("PawnPic");
            this.pawnDeadObject = new GameObject("ded");
            this.pawnNameObject = new GameObject("name");

            this.pawnBackgroundObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);
            this.moodBarObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);
            this.pawnPortraitObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);
            this.pawnDeadObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);
            this.pawnNameObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);

            this.pawnBackground = this.pawnBackgroundObject.AddComponent<RawImage>();
            this.moodBar = this.moodBarObject.AddComponent<RawImage>();
            this.pawnPortrait = this.pawnPortraitObject.AddComponent<RawImage>();
            this.pawnDead = this.pawnDeadObject.AddComponent<RawImage>();
            this.pawnName = this.pawnNameObject.AddComponent<Text>();

            this.pawnBackground.rectTransform.pivot = Vector2.zero;
            this.pawnPortrait.rectTransform.pivot = Vector2.zero;
            this.moodBar.rectTransform.pivot = Vector2.zero;
            this.pawnDead.rectTransform.pivot = Vector2.zero;
            this.pawnName.rectTransform.pivot = new Vector2(0.5f, 0);

            this.pawnBackground.texture = ColonistBar.BGTex;
            this.pawnBackground.raycastTarget = true;

            this.moodBar.texture = BaseContent.WhiteTex;
            this.pawnName.font = (Font)Resources.Load("Fonts/Calibri_tiny");
            this.pawnName.alignment = TextAnchor.MiddleCenter;
            this.pawnName.fontSize = 12;
            this.pawnName.raycastTarget = false;

            // Handle clicks on this entry
            this.colonistHighlighter = this.pawnPortraitObject.AddComponent<ColonistHighlighter>();
        }

        /// <summary>
        /// Free resources associated with this entry.
        /// </summary>
        public void Destroy()
        {
            Object.Destroy(this.pawnBackgroundObject);
            Object.Destroy(this.moodBarObject);
            Object.Destroy(this.pawnPortraitObject);
            Object.Destroy(this.pawnDeadObject);
            Object.Destroy(this.pawnNameObject);

            foreach (ColonistBarItemIcon colonistBarItemIcon in this.iconsShown)
            {
                colonistBarItemIcon.Destroy();
            }
        }

        /// <summary>
        /// Trigger an update of the cached portrait for this entry as needed.
        /// </summary>
        /// <param name="pawn">Pawn to update portrait for.</param>
        /// <returns>Whether this entry belonged to the given pawn.</returns>
        public bool UpdatePortrait(Pawn pawn)
        {
            if (pawn.thingIDNumber == this.pawnId)
            {
                this.pawnId = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Main render function, invoked on each frame while the game is running.
        /// </summary>
        /// <param name="pawn">Pawn to render in this item.</param>
        /// <param name="curIndex">Index of this item within its respective row.</param>
        /// <param name="curRow">Row index of this item.</param>
        /// <param name="curScaleFactor">Current scale factor applied to the colonist bar.</param>
        public void Update(Pawn pawn, int curIndex, int curRow, float curScaleFactor)
        {
            if (curIndex != this.index || curRow != this.row || curScaleFactor != this.scaleFactor)
            {
                this.index = curIndex;
                this.row = curRow;
                this.scaleFactor = curScaleFactor;

                float horPos = 36 + (curIndex * curScaleFactor * (ColonistBarItem.ItemSpacing + ColonistBarColonistDrawer.PawnTextureSize.x)) - (Find.Camera.pixelWidth / 2);

                this.offset = new Vector2(horPos, (UI.screenHeight / 2) - (curScaleFactor * curRow * ColonistBarColonistDrawer.PawnTextureSize.y));
                Vector2 textOffset = this.offset - new Vector2(-curScaleFactor * ColonistBar.BaseSize.x / 2, ColonistBar.BaseSize.y + 12f);

                this.pawnBackground.rectTransform.anchoredPosition = this.offset;
                this.pawnPortrait.rectTransform.anchoredPosition = this.offset;
                this.moodBar.rectTransform.anchoredPosition = this.offset;
                this.pawnDead.rectTransform.anchoredPosition = this.offset;
                this.pawnName.rectTransform.anchoredPosition = textOffset;

                this.pawnBackground.rectTransform.sizeDelta = ColonistBar.BaseSize * curScaleFactor;
                this.pawnPortrait.rectTransform.sizeDelta = ColonistBarColonistDrawer.PawnTextureSize * curScaleFactor;
            }

            float? curMood = pawn.needs?.mood?.CurLevelPercentage;
            if (curMood != this.mood)
            {
                this.mood = curMood;
                float height = (this.mood ?? 1) * ColonistBar.BaseSize.y;
                this.moodBar.color = this.GetMoodColor(curMood, pawn.mindState?.mentalBreaker);
                this.moodBar.rectTransform.sizeDelta = new Vector2(curScaleFactor * ColonistBar.BaseSize.x, curScaleFactor * height);
            }

            string curName = pawn.LabelShortCap;
            if (curName != this.name)
            {
                this.name = curName;
                this.pawnName.text = curName;
            }

            if (pawn.thingIDNumber != this.pawnId)
            {
                this.pawnId = pawn.thingIDNumber;

                this.colonistHighlighter.Pawn = pawn;

                Texture2D prevPortraitTexture = this.pawnPortrait.texture as Texture2D;

                // It doesn't seem possible to directly render cached portrait textures without
                // a weird issue whereby portraits get mixed up when new pawns spawn in.
                // So, create a temporary texture as needed and render that instead.
                RenderTexture newPortraitTexture = PortraitsCache.Get(
                  pawn,
                  ColonistBarColonistDrawer.PawnTextureSize,
                  Rot4.South,
                  ColonistBarColonistDrawer.PawnTextureCameraOffset,
                  1.28205f);
                Texture2D dest = new Texture2D(newPortraitTexture.width, newPortraitTexture.height, TextureFormat.RGBA32, false);
                Graphics.CopyTexture(newPortraitTexture, dest);
                this.pawnPortrait.texture = dest;

                // Ensure we clean up after any previous temporary textures
                if (prevPortraitTexture != null)
                {
                    Object.Destroy(prevPortraitTexture);
                }
            }

            bool isDeadNow = pawn.Dead;

            if (isDeadNow != this.dead)
            {
                this.dead = isDeadNow;
                this.pawnDead.texture = ColonistBarColonistDrawer.DeadColonistTex;
                this.pawnDead.rectTransform.sizeDelta = ColonistBar.BaseSize * curScaleFactor;
                this.pawnDeadObject.SetActive(isDeadNow);
            }

            int numIcons = 0;
            int prevIconCount = this.iconsShown.Count;
            foreach (Texture iconTexture in this.GetActiveIcons(pawn))
            {
                if (numIcons < prevIconCount)
                {
                    this.iconsShown[numIcons].Update(iconTexture, this.offset, numIcons, curScaleFactor);
                }
                else
                {
                    ColonistBarItemIcon colonistBarItemIcon = new ColonistBarItemIcon(this.canvasObject);
                    colonistBarItemIcon.Update(iconTexture, this.offset, numIcons, curScaleFactor);
                    this.iconsShown.Add(colonistBarItemIcon);
                }

                numIcons++;
            }

            for (int i = this.iconsShown.Count - 1; i >= numIcons; i--)
            {
                ColonistBarItemIcon colonistBarItemIcon = this.iconsShown[i];
                colonistBarItemIcon.Destroy();
                this.iconsShown.RemoveAt(i);
            }
        }

        /// <summary>
        /// Determine the appropriate background color to use based on the pawn's current mood.
        /// </summary>
        private Color GetMoodColor(float? curMood, MentalBreaker mentalBreaker)
        {
            if (curMood == null || curMood > mentalBreaker.BreakThresholdMinor)
            {
                return NormalMoodColor;
            }

            return MinorBreakColor;
        }

        private IEnumerable<Texture> GetActiveIcons(Pawn pawn)
        {
            JobDef curJobDef = pawn.CurJobDef;

            if (curJobDef == JobDefOf.AttackMelee || curJobDef == JobDefOf.AttackStatic || (curJobDef == JobDefOf.Wait_Combat && pawn.stances.curStance is Stance_Busy busyStance && busyStance.focusTarg.IsValid))
            {
                yield return ColonistBarColonistDrawer.Icon_Attacking;
            }
            else if (curJobDef == JobDefOf.FleeAndCower)
            {
                yield return ColonistBarColonistDrawer.Icon_Fleeing;
            }
            else if (pawn.mindState.IsIdle)
            {
                yield return ColonistBarColonistDrawer.Icon_Idle;
            }

            if (pawn.IsBurning())
            {
                yield return ColonistBarColonistDrawer.Icon_Burning;
            }

            if (pawn.Inspired)
            {
                yield return ColonistBarColonistDrawer.Icon_Inspired;
            }
        }
    }
}
