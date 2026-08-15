using System;
using UnityEngine;

namespace ProceduralMap
{
    [Serializable]
    public sealed class StomachRockDefinition
    {
        public string name = "StomachRock";
        public GameObject prefab;
        [Min(1)] public int maximumCount = 30;
        [Min(0)] public int minimumSpacing = 2;
        public Vector2Int footprint = Vector2Int.one;
        public Vector2 positionOffset = new Vector2(0.5f, 0.5f);
        [Min(0.001f)] public float noiseScale = 0.04f;
        [Range(0f, 1f)] public float noiseThreshold = 0.55f;
        [Range(0f, 1f)] public float placementChance = 0.35f;
        public bool randomFlipX = true;
    }
}
