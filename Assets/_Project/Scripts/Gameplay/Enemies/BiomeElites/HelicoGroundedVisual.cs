using UnityEngine;
using System.Collections.Generic;
namespace Necrocis
{
    // Ground the thick body's underside, not its image centroid or trailing flags.
    // The footprint stays in gameplay space; only the sprite's presentation moves.
    [DefaultExecutionOrder(200)]
    public sealed class HelicoGroundedVisual : MonoBehaviour
    {
        private SpriteRenderer body;
        private Rigidbody physicsBody;
        private HelicoSpiralElitePattern pattern;
        private readonly Dictionary<Sprite, float> bottoms = new Dictionary<Sprite, float>();
        public Vector3 GroundContactPosition { get; private set; }
        public float CurrentBodyBottomY { get; private set; }
        public void Configure(SpriteRenderer renderer, HelicoSpiralPresentation art, HelicoSpiralElitePattern owner)
        {
            body = renderer; pattern = owner; physicsBody = GetComponent<Rigidbody>(); bottoms.Clear();
            body.GetComponent<SpriteYSort>()?.SetSortingAnchor(physicsBody);
            foreach (var entry in art.groundings) bottoms.Add(entry.sprite, entry.bodyBottomY);
            enabled = true; LateUpdate();
        }
        private void LateUpdate()
        {
            var camera = DontStarveCamera.GetActiveCamera();
            if (body == null || body.sprite == null || pattern == null || camera == null || camera.transform.forward.y >= -.1f) return;
            if (!bottoms.TryGetValue(body.sprite, out float bottom)) return;
            body.transform.rotation = camera.transform.rotation;
            Vector3 ground = physicsBody != null ? physicsBody.position : transform.position;
            if (BiomeManager.Active != null) ground.y = BiomeManager.Active.GetGroundHeight(ground);
            Vector3 screenDepth = Vector3.ProjectOnPlane(camera.transform.up, Vector3.up).normalized;
            float nearAxis = pattern.BodyHalfLength * Mathf.Abs(Vector3.Dot(pattern.BodyDirection, screenDepth));
            GroundContactPosition = ground - screenDepth * nearAxis + Vector3.up * .025f;
            CurrentBodyBottomY = bottom;
            Vector3 position = GroundContactPosition - camera.transform.up * (bottom * body.transform.lossyScale.y);
            // Flags may extend below the thick body. A view-ray lift prevents floor
            // clipping without shifting the measured ground contact on screen.
            float lowest = position.y + body.sprite.bounds.min.y * body.transform.lossyScale.y * camera.transform.up.y;
            float lift = Mathf.Max(0, ground.y + .025f - lowest);
            body.transform.position = position + camera.transform.forward * (lift / camera.transform.forward.y);
        }
    }
}
