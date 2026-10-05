using System;
using UnityEngine;

namespace NecrocisEditor
{
    internal static class ElitePresentationChecks
    {
        public static void RedArea(GameObject marker, Vector2 expectedSize)
        {
            Transform area = marker.transform.Find("DamageArea");
            if (area == null) throw new InvalidOperationException("Full damage-area background is missing");
            var renderer = area.GetComponent<MeshRenderer>(); var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
            Color color = block.GetColor("_Color");
            if (color.r < .7f || color.g > .25f || color.b > .25f || color.a < .2f || color.a >= 1)
                throw new InvalidOperationException("Danger background must be visible translucent red: " + color);
            Vector3 size = renderer.bounds.size;
            if (Mathf.Abs(size.x - expectedSize.x) > .002f || Mathf.Abs(size.z - expectedSize.y) > .002f)
                throw new InvalidOperationException($"Danger background does not cover the complete hit area: {size}, expected {expectedSize}");
            if (area.GetComponent<Collider>() != null) throw new InvalidOperationException("Danger background must not add collision");
        }
    }
}
