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

using RimWorld.Planet;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;

namespace UnityBar
{
    /// <summary>
    /// Click handler for a given colonist bar item that manages highlight rendering and navigation.
    /// </summary>
    [HotSwappable]
    public class ColonistHighlighter : MonoBehaviour, IPointerClickHandler
    {
        private Outline highlightOutline;

        public Pawn Pawn { get; set; }

        public void Start()
        {
            this.highlightOutline = this.gameObject.AddComponent<Outline>();
            this.highlightOutline.effectColor = Color.white;
            this.highlightOutline.effectDistance = new Vector2(3f, 3f);

            this.highlightOutline.enabled = false;
        }

        /// <summary>
        /// Handle mouse clicks on this colonist bar item.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            ColonistBarEvents.HighlightPawn(this.Pawn);

            eventData.Use();

            if (eventData.clickCount <= 1)
            {
                return; // single click case
            }

            if (WorldRendererUtility.WorldRenderedNow)
            {
                CameraJumper.TrySelect(this.Pawn);
            }
            else
            {
                CameraJumper.TryJumpAndSelect(this.Pawn);
            }
        }

        public void OnEnable()
        {
            ColonistBarEvents.HighlightedPawnChanged += this.OnHighlightedPawnChanged;
        }

        public void OnDisable()
        {
            ColonistBarEvents.HighlightedPawnChanged -= this.OnHighlightedPawnChanged;
        }

        /// <summary>
        /// Enable/disable the highlight outline for this entry when the highlighted pawn changes.
        /// </summary>
        /// <param name="highlighted">Pawn that was highlighted in the colonist bar.</param>
        private void OnHighlightedPawnChanged(Pawn highlighted)
        {
            this.highlightOutline.enabled = this.Pawn == highlighted;
        }
    }
}
