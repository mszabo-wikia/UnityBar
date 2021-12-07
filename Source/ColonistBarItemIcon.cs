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

using RimWorld;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnityBar
{
    /// <summary>
    /// Manage rendering for a single colonist bar item icon (e.g. inspiration, mental state etc.)
    /// </summary>
    [HotSwappable]
    public class ColonistBarItemIcon
    {
        private readonly GameObject iconObject;
        private readonly RawImage iconImage;
        private float scaleFactor;
        private int? index;

        public ColonistBarItemIcon(GameObject canvasObject)
        {
            this.iconObject = new GameObject();
            this.iconImage = this.iconObject.AddComponent<RawImage>();

            this.iconImage.transform.SetParent(canvasObject.transform, worldPositionStays: false);
            this.iconImage.rectTransform.pivot = Vector2.zero;
        }

        /// <summary>
        /// Main render function, invoked on each frame while the game is running.
        /// </summary>
        /// <param name="iconTexture">Texture to render for this icon.</param>
        /// <param name="parentOffset">Offset of the colonist bar item holding this icon.</param>
        /// <param name="curIndex">Index of this icon within the list of icons for this colonist bar item.</param>
        /// <param name="curScaleFactor">Current scale factor applied to the colonist bar.</param>
        public void Update(
            Texture iconTexture,
            Vector2 parentOffset,
            int curIndex,
            float curScaleFactor)
        {
            this.iconImage.texture = iconTexture;

            if (this.index != curIndex || this.scaleFactor != curScaleFactor)
            {
                this.index = curIndex;
                this.scaleFactor = curScaleFactor;

                float iconSize = curScaleFactor * ColonistBarColonistDrawer.BaseIconMaxSize;

                this.iconImage.rectTransform.anchoredPosition = new Vector2(
                    parentOffset.x + (curIndex * iconSize),
                    parentOffset.y);
                this.iconImage.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
            }
        }

        /// <summary>
        /// Free resources associated with this icon.
        /// </summary>
        public void Destroy() => Object.Destroy(this.iconObject);
    }
}
