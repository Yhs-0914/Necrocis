using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField]
    private Transform playerTransform;

    [Header("카메라 설정")]
    [Tooltip("플레이어를 기준으로 카메라가 얼마나 떨어져 있을지 설정합니다.")]
    [SerializeField]
    private Vector3 offset = new Vector3(0, 10, -5);

    // Update와 HandleRotation 함수는 더 이상 필요 없으므로 삭제합니다.

    void LateUpdate()
    {
        if (playerTransform == null)
        {
            return;
        }

        // 플레이어의 회전값(playerTransform.rotation)을 offset에 곱해줍니다.
        // 이를 통해 카메라는 항상 플레이어의 '등 뒤'에 위치하게 됩니다.
        Vector3 desiredPosition = playerTransform.position + playerTransform.rotation * offset;
        transform.position = desiredPosition;

        // 카메라가 항상 플레이어를 바라보도록 합니다.
        transform.LookAt(playerTransform);
    }
}