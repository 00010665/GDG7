using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.UI
{
    /// <summary>Loads the generated food-themed sprites from Resources/UI/Icons.</summary>
    public static class FoodIconLibrary
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return null;
            if (!Cache.TryGetValue(assetName, out Sprite sprite))
            {
                sprite = Resources.Load<Sprite>($"UI/Icons/{assetName}");
                Cache[assetName] = sprite;
            }
            return sprite;
        }
    }
}
