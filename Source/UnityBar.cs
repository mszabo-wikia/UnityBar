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
using System.Linq;
using RimWorld;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;

namespace UnityBar
{
    /// <summary>
    /// Main colonist bar manager.
    /// </summary>
    [HotSwappable]
    public class UnityBar : MonoBehaviour
    {
        private const int HorizontalPadding = 36;

        private readonly List<Pawn> pawns = new List<Pawn>();
        private readonly List<ColonistBarItem> items = new List<ColonistBarItem>();
        private readonly float itemWidth = ColonistBar.BaseSize.x + ColonistBarItem.ItemSpacing;

        private bool entriesDirty = true;
        private float scaleFactor = 1f;
        private int itemsPerRow;
        private GameObject canvasObject;

        public void Start()
        {
            this.canvasObject = new GameObject("Canvas");
            Canvas canvas = this.canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            this.canvasObject.AddComponent<GraphicRaycaster>();

            GameObject evObject = new GameObject("events");
            evObject.AddComponent<EventSystem>();
            evObject.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// Update the colonist bar, called on every frame.
        /// </summary>
        public void Update()
        {
            if (Find.TickManager.Paused)
            {
                return;
            }

            // Recalculate constituent pawns when requested
            if (this.entriesDirty)
            {
                this.pawns.Clear();
                foreach (Map aMap in Find.Maps.OrderBy(map => !map.IsPlayerHome).ThenBy(map => map.uniqueID))
                {
                    this.pawns.AddRange(aMap.mapPawns.FreeColonists);

                    foreach (Thing corpseThing in aMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse))
                    {
                        if (corpseThing is Corpse aCorpse && !aCorpse.IsDessicated() && aCorpse.InnerPawn?.IsColonist == true)
                        {
                            this.pawns.Add(aCorpse.InnerPawn);
                        }
                    }
                }

                this.scaleFactor = this.itemWidth * this.pawns.Count >= UI.screenWidth / 2 ? 0.8f : 1f;
                float scaledItemWidth = (this.scaleFactor * ColonistBar.BaseSize.x) + ColonistBarItem.ItemSpacing;
                int availableWidth = (UI.screenWidth - 2) * HorizontalPadding;
                this.itemsPerRow = Mathf.FloorToInt(availableWidth / scaledItemWidth);
                this.entriesDirty = false;
            }

            int curRow = 0;
            int prevItemCount = this.items.Count;

            for (int i = 0; i < this.pawns.Count; i++)
            {
                int indexInRow = i % this.itemsPerRow;
                if (indexInRow == 0)
                {
                    curRow++;
                }

                if (i < prevItemCount)
                {
                    this.items[i].Update(this.pawns[i], indexInRow, curRow, this.scaleFactor);
                }
                else
                {
                    // An extra pawn was added in this update, so ensure it's added to the bar
                    ColonistBarItem item = new ColonistBarItem(this.canvasObject);
                    item.Update(this.pawns[i], indexInRow, curRow, this.scaleFactor);
                    this.items.Add(item);
                }
            }

            // Pop off extraneous pawns from the bar
            for (int i = this.items.Count - 1; i >= this.pawns.Count; i--)
            {
                ColonistBarItem colonistBarItem = this.items[i];
                this.items.RemoveAt(i);
                colonistBarItem.Destroy();
            }
        }

        /// <summary>
        /// Recalculate the list of pawns to include on the next frame.
        /// </summary>
        public void MarkDirty()
        {
            this.entriesDirty = true;
        }

        /// <summary>
        /// Trigger an update of the cached portrait for a given pawn.
        /// </summary>
        /// <param name="pawn">Pawn to update portrait for.</param>
        public void UpdatePortrait(Pawn pawn)
        {
            foreach (ColonistBarItem item in this.items)
            {
                if (item.UpdatePortrait(pawn))
                {
                    return; // found, stop processing
                }
            }
        }
    }
}
