using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private Animation animationComponent;

    private CharacterController controller;

    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = IsGrounded();

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = -transform.right * x - transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Rotate body if moving
        if (move.magnitude > 0.1f)
            body.forward = move.normalized;

        // Set animations based on movement
        if (isGrounded)
        {
            if (move.magnitude > 0.1f)
            {
                if (!animationComponent.IsPlaying("walk"))
                {
                    animationComponent.Play("walk"); // Play walk animation
                }
            }
            else
            {
                if (!animationComponent.IsPlaying("idle"))
                {
                    animationComponent.Play("idle"); // Play idle animation
                }
            }
        }
        else
        {
            if (!animationComponent.IsPlaying("jump"))
            {
                animationComponent.Play("jump"); // Play jump animation
            }
        }

        // Jump logic
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        float rayLength = 0.3f; // tweak this if needed
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // slight offset to avoid starting inside ground

        return Physics.Raycast(rayOrigin, Vector3.down, rayLength);
    }
}
