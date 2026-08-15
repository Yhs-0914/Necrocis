using System;
using UnityEngine;

namespace ProceduralMap
{
    [Serializable]
    public sealed class LavaPopDefinition
    {
        public string name = "Lava Pop";
        public GameObject prefab;
        [Min(0)] public int maximumCount = 60;
        [Min(1)] public int minimumSpacing = 3;
        public Vector2 positionOffset = new Vector2(0.5f, 0.5f);
    }
}
