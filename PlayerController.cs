using UnityEngine;
using System.Collections;

/// <summary>
/// Oyuncu hareketini yönetir: lane değiştirme, zıplama, kayma ve swipe/klavye input.
/// CharacterController component'i gerektirir.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Ayarları")]
    public float laneDistance = 3f;      // Lane'ler arası mesafe
    private int currentLane = 1;         // 0 = sol, 1 = orta, 2 = sağ
    public float laneChangeSpeed = 12f;

    [Header("Zıplama & Kayma")]
    public float jumpForce = 9f;
    public float gravity = -25f;
    public float slideDuration = 0.8f;
    private bool isSliding = false;
    private bool isJumping = false;

    [Header("Swipe Ayarları")]
    public float minSwipeDistance = 50f;

    [Header("Animasyon (opsiyonel)")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 targetPosition;
    private CapsuleCollider capsuleCollider;
    private float originalHeight;
    private Vector3 originalCenter;

    private Vector2 touchStartPos;
    private bool isTouching = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            originalHeight = capsuleCollider.height;
            originalCenter = capsuleCollider.center;
        }
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleInput();
        HandleMovement();
        HandleGravity();
    }

    private void HandleInput()
    {
        // --- Klavye (test için, PC editör) ---
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) ChangeLane(1);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Slide();

        // --- Dokunmatik Swipe (mobil) ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Ended && isTouching)
            {
                Vector2 delta = touch.position - touchStartPos;
                isTouching = false;

                if (delta.magnitude < minSwipeDistance) return;

                float absX = Mathf.Abs(delta.x);
                float absY = Mathf.Abs(delta.y);

                if (absX > absY)
                {
                    // Yatay swipe
                    ChangeLane(delta.x > 0 ? 1 : -1);
                }
                else
                {
                    // Dikey swipe
                    if (delta.y > 0) Jump();
                    else Slide();
                }
            }
        }
    }

    private void ChangeLane(int direction)
    {
        int newLane = Mathf.Clamp(currentLane + direction, 0, 2);
        if (newLane == currentLane) return;

        currentLane = newLane;
        float xPos = (currentLane - 1) * laneDistance;
        targetPosition = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    private void HandleMovement()
    {
        // X ekseninde hedef lane'e yumuşak geçiş
        Vector3 pos = transform.position;
        float newX = Mathf.Lerp(pos.x, targetPosition.x, Time.deltaTime * laneChangeSpeed);

        // Z ekseninde ileri hareket (dünya oyuncuya doğru gelmiyorsa, oyuncu ileri gider)
        float forwardSpeed = GameManager.Instance.currentSpeed;

        Vector3 move = new Vector3(newX - pos.x, 0, forwardSpeed * Time.deltaTime);
        controller.Move(move);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;
            isJumping = false;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);

        if (animator != null)
            animator.SetBool("IsGrounded", controller.isGrounded);
    }

    private void Jump()
    {
        if (isJumping || isSliding || !controller.isGrounded) return;

        isJumping = true;
        velocity.y = jumpForce;
        if (animator != null) animator.SetTrigger("Jump");
    }

    private void Slide()
    {
        if (isSliding || isJumping) return;
        StartCoroutine(SlideRoutine());
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;
        if (animator != null) animator.SetTrigger("Slide");

        // Collider'ı küçültüp altından geçebilmesini sağla
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalHeight * 0.5f;
            capsuleCollider.center = new Vector3(originalCenter.x, originalCenter.y * 0.5f, originalCenter.z);
        }

        yield return new WaitForSeconds(slideDuration);

        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalHeight;
            capsuleCollider.center = originalCenter;
        }

        isSliding = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}
