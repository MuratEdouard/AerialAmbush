using System.Collections;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Events;
using Animation = UnityEngine.Animation;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    public UnityEvent playerDied;

    public Transform body;
    public Animation animationComponent;
    public AudioSource walkingSound;
    public AudioSource jumpSound;
    public AudioSource dashSound;
    public AudioSource landSound;
    public AudioSource hurtSound;
    public ParticleSystem dustParticles;

    public float dashCooldown = 2f;
    public float normalSpeed = 2f;
    public float dashSpeed = 25f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;
    public LayerMask floorMask;

    private Rigidbody rigidBody;

    private bool isFrozen = true;
    private bool isDashing = false;
    private bool isGrounded = false;
    private bool isDying = false;

    private bool canDash = true;
    private bool canJump = true;

    private float speed;
    private float jumpVelocity;

    private Vector3 inputVector;
    private bool jumpPressed;
    private bool dashPressed;

    void Start()
    {
        speed = normalSpeed;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        rigidBody.interpolation = RigidbodyInterpolation.Interpolate;

        jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Update()
    {
        if (isFrozen || isDying)
            return;

        HandleInputs();
    }

    void FixedUpdate()
    {
        if (isFrozen || isDying)
            return;

        isGrounded = CheckFloor();
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleOneWayPlatform();
    }

    private void HandleInputs()
    {
        // Read input
        inputVector = new Vector3(-Input.GetAxisRaw("Horizontal"), 0, -Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
        if (Input.GetButtonDown("Fire1"))
            dashPressed = true;
    }

    private void HandleMovement()
    {
        if (isDashing)
        {
            Vector3 move = body.forward * speed;
            Vector3 targetVelocity = new Vector3(move.x, rigidBody.linearVelocity.y, move.z);
            rigidBody.linearVelocity = targetVelocity;
        }
        else
        {

            Vector3 move = inputVector * speed;
            Vector3 targetVelocity = new Vector3(move.x, rigidBody.linearVelocity.y, move.z);
            rigidBody.linearVelocity = targetVelocity;


            // Rotate body toward movement direction
            if (move.magnitude > 0.1f)
            {
                body.forward = move.normalized;
            }


            if (isGrounded)
            {
                canJump = true;

                if (move.magnitude > 0.1)
                {
                    // Walk
                    if (!animationComponent.IsPlaying("walk"))
                    {
                        animationComponent.Play("walk");
                        walkingSound.Play();
                        dustParticles.Play();
                    }
                }
                else
                {
                    // Idle
                    if (!animationComponent.IsPlaying("idle"))
                    {
                        animationComponent.Play("idle");
                        if (walkingSound.isPlaying)
                        {
                            walkingSound.Stop();
                            dustParticles.Stop();
                        }
                    }
                }
            }
            else
            {
                // Falling or Jumping
                if (walkingSound.isPlaying)
                {
                    walkingSound.Stop();
                    dustParticles.Stop();
                }
            }

            float y = rigidBody.linearVelocity.y + gravity * Time.fixedDeltaTime;
            rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, y, rigidBody.linearVelocity.z);
        }
    }

    private void HandleJump()
    {
        if(canJump && jumpPressed)
        {
            jumpPressed = false;
            if(!isGrounded)
                canJump = false;

            rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, jumpVelocity, rigidBody.linearVelocity.z);
            jumpSound.Play();

            if (!animationComponent.IsPlaying("jump"))
                animationComponent.Play("jump");

            LeanTween.scale(body.gameObject, new Vector3(1.0f, 1.5f, 1.0f), 0.2f);
            Invoke(nameof(ResetBody), 0.2f);

        }
    }

    private void HandleDash()
    {
        if(canDash && dashPressed)
        {
            dashPressed = false;
            canDash = false;
            isDashing = true;
            speed = dashSpeed;
            dashSound.Play();
            LeanTween.scale(body.gameObject, new Vector3(1.0f, 1.0f, 1.5f), 0.2f);
            Invoke(nameof(ResetDash), 0.2f);
        }
    }

    private void HandleOneWayPlatform()
    {
        if (rigidBody.linearVelocity.y > 0f)
        {
            rigidBody.excludeLayers = floorMask;
        }
        else
        {
            rigidBody.excludeLayers = 0;
        }
    }

    private bool CheckFloor()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f; // Start slightly above the character
        return Physics.Raycast(origin, Vector3.down, out hit, 0.2f, floorMask);
    }

    private void ResetDash()
    {
        canDash = true;
        isDashing = false;
        speed = normalSpeed;
        ResetBody();
    }

    private void ResetBody()
    {
        LeanTween.scale(body.gameObject, Vector3.one, 0.2f);
    }

    public void Die()
    {
        walkingSound.Stop();
        dustParticles.Stop();

        isDying = true;
        animationComponent.Play("die");
        hurtSound.Play();

        playerDied.Invoke();
    }

    public void Freeze()
    {
        isFrozen = true;
        rigidBody.isKinematic = true;
    }

    public void Unfreeze()
    {
        isFrozen = false;
        rigidBody.isKinematic = false;
    }

    public void OnWin()
    {
        animationComponent.Play("idle");
        if (walkingSound.isPlaying)
        {
            walkingSound.Stop();
            dustParticles.Stop();
        }
        Freeze();
    }

    public void OnLose()
    {
        Freeze();
    }
}
