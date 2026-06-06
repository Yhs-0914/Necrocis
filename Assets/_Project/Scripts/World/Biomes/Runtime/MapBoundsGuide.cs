using UnityEngine;

namespace Necrocis
{
    [ExecuteAlways]
    public sealed class MapBoundsGuide : MonoBehaviour
    {
        [SerializeField] private int width = 300;
        [SerializeField] private int height = 300;
        [SerializeField] private Vector2 center;
        [SerializeField] private Color borderColor = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Color halfLineColor = new Color(1f, 1f, 0f, 0.8f);

        private void OnDrawGizmos()
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            Vector3 bottomLeft = transform.TransformPoint(new Vector3(center.x - halfWidth, center.y - halfHeight, 0f));
            Vector3 bottomRight = transform.TransformPoint(new Vector3(center.x + halfWidth, center.y - halfHeight, 0f));
            Vector3 topRight = transform.TransformPoint(new Vector3(center.x + halfWidth, center.y + halfHeight, 0f));
            Vector3 topLeft = transform.TransformPoint(new Vector3(center.x - halfWidth, center.y + halfHeight, 0f));

            Gizmos.color = borderColor;
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);

            Gizmos.color = halfLineColor;
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(center.x - halfWidth, center.y, 0f)),
                transform.TransformPoint(new Vector3(center.x + halfWidth, center.y, 0f)));
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(center.x, center.y - halfHeight, 0f)),
                transform.TransformPoint(new Vector3(center.x, center.y + halfHeight, 0f)));
        }
    }
}
