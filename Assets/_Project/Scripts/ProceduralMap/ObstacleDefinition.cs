using System;
using UnityEngine;

namespace ProceduralMap
{
    /// <summary>장애물 한 종류의 Prefab과 배치 규칙.</summary>
    [Serializable]
    public sealed class ObstacleDefinition
    {
        public string name = "Obstacle";
        public GameObject prefab;

        [Header("Allowed Terrain")]
        public bool allowBase = true;
        public bool allowGrass = true;
        public bool allowSecondFloor;

        [Header("Perlin Density")]
        [Min(0.001f)] public float noiseScale = 0.03f;
        [Range(0f, 1f)] public float noiseThreshold = 0.6f;
        [Range(0f, 1f)] public float placementChance = 0.35f;

        [Header("Placement")]
        [Min(0)] public int maximumCount = 100;
        [Min(0)] public int minimumSpacing = 2;
        [Tooltip("절벽으로부터 확보할 최소 셀 간격")]
        [Min(0)] public int cliffClearance = 2;
        public Vector2Int footprint = Vector2Int.one;
        public Vector2 positionOffset;
        public bool randomFlipX;
    }
}
