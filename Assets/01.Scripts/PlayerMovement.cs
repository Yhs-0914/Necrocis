using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 moveInput;
    
    // 마지막 이동 방향 저장 (public으로 다른 스크립트에서 접근)
    public Vector3 lastMoveDirection { get; private set; }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        lastMoveDirection = Vector3.forward; // 초기값: 앞
    }
    
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveInput = new Vector3(horizontal, 0, vertical).normalized;
        
        // 이동 중이면 마지막 방향 저장
        if (moveInput != Vector3.zero)
        {
            lastMoveDirection = moveInput;
        }
        
        float currentSpeed = PlayerStats.Instance.GetSpeed();
        controller.Move(moveInput * currentSpeed * Time.deltaTime);
        
        controller.Move(Vector3.down * 9.8f * Time.deltaTime);
    }
}