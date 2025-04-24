using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    public Transform body;
    public Animation animationComponent;
    public AudioSource walkingSound;
    public AudioSource jumpSound;
    public AudioSource dashSound;
    public AudioSource landSound;
    public AudioSource hurtSound;
    public ParticleSystem dustParticles;

    public float dashCooldown = 2f;
    public float normalSpeed = 5f;
    public float dashSpeed = 15f;
    public float gravity = -20f;
    public float jumpHeight = 2f;
    public LayerMask floorMask;

    private Rigidbody rigidBody;
    private Collider collider;
    private bool isFloored = false;
    private bool isWalking = false;
    private bool isDashing = false;
    private bool justJumped = false;
    private bool canDash = true;
    private bool canDoubleJump = true;
    private float speed;
    private float jumpVelocity;

    void Start()
    {
        speed = normalSpeed;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
        collider = GetComponent<Collider>();

        jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Update()
    {
        HandleMovement();
        HandleDustParticles();
        HandleSounds();
    }

    private void HandleMovement()
    {
        isFloored = CheckFloored();

        Vector3 input = new Vector3(-Input.GetAxisRaw("Horizontal"), 0, -Input.GetAxisRaw("Vertical"));
        Vector3 move = input.normalized * speed;

        // Force moving forward when dashing (no turning or reduction of speed)
        if (isDashing)
        {
            speed = dashSpeed;
            move = body.forward * speed;
        }

        // Player movement
        Vector3 targetVelocity = new Vector3(move.x, rigidBody.linearVelocity.y, move.z);
        rigidBody.linearVelocity = targetVelocity;


        // Rotate body toward movement direction
        isWalking = false;
        if (move.magnitude > 0.1f)
        {
            body.forward = move.normalized;
            if (isFloored)
            {
                isWalking = true;
            }
        }

        // Floored animations
        if (isFloored && !justJumped)
        {
            canDoubleJump = true;

            if (isWalking)
            {
                if (!animationComponent.IsPlaying("walk")) animationComponent.Play("walk");
            }
            else if (!animationComponent.IsPlaying("idle"))
            {
                animationComponent.Play("idle");
            }

            if (Input.GetButtonDown("Jump"))
            {
                rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, jumpVelocity, rigidBody.linearVelocity.z);
                justJumped = true;
                Invoke(nameof(afterJustJumped), 0.2f);

                LeanTween.scale(body.gameObject, new Vector3(1.0f, 1.5f, 1.0f), 0.2f);
                Invoke(nameof(ResetBody), 0.2f);
            }
        }
        else
        {
            if (!animationComponent.IsPlaying("jump"))
                animationComponent.Play("jump");
        }

        // Double Jump
        if (!isFloored && canDoubleJump && Input.GetButtonDown("Jump"))
        {
            canDoubleJump = false;

            rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, jumpVelocity, rigidBody.linearVelocity.z);
            justJumped = true;
            Invoke(nameof(afterJustDoubleJumped), 0.2f);

            LeanTween.scale(body.gameObject, new Vector3(1.0f, 1.5f, 1.0f), 0.2f);
            LeanTween.rotateAroundLocal(body.gameObject, Vector3.right, 360.0f, 0.2f);

            Invoke(nameof(ResetBody), 0.2f);
        }

        // Dashing
        if (Input.GetButtonDown("Fire1") && canDash)
        {
            canDash = false;
            isDashing = true;
            dashSound.Play();
            LeanTween.scale(body.gameObject, new Vector3(1.0f, 1.0f, 1.5f), 0.2f);
            Invoke(nameof(ResetDash), 0.2f);
        }

        // Remove gravity when on floor and not at the beginning of a jump
        Vector3 vel = rigidBody.linearVelocity;
        if (isFloored && !justJumped)
        {
            rigidBody.useGravity = false;
            vel.y = 0f;
        }
        else
        {
            rigidBody.useGravity = true;
            vel.y += gravity * Time.deltaTime;
        }
        rigidBody.linearVelocity = vel;

        // Disable collisions when moving upwards
        collider.enabled = (rigidBody.linearVelocity.y <= 0f);
    }

    private void HandleDustParticles()
    {
        if (isWalking && dustParticles.isStopped)
            dustParticles.Play();
        else if (!isWalking && dustParticles.isPlaying)
            dustParticles.Stop();
    }

    private void HandleSounds()
    {
        if (isWalking && !walkingSound.isPlaying)
            walkingSound.Play();
        else if (!isWalking && walkingSound.isPlaying)
            walkingSound.Stop();

        if (justJumped && !jumpSound.isPlaying)
            jumpSound.Play();
    }

    private bool CheckFloored()
    {
        if (rigidBody.linearVelocity.y <= 0f)
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            return Physics.CheckSphere(origin + Vector3.down * 0.15f, 0.2f, floorMask);
        }
        else
        {
            return false;
        }
            
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
        Vector3 bodyAngles = body.gameObject.transform.localEulerAngles;
        bodyAngles.x = 0f;
        body.gameObject.transform.localEulerAngles = bodyAngles;
    }

    private void afterJustJumped()
    {
        justJumped = false;
        canDoubleJump = true;
    }

    private void afterJustDoubleJumped()
    {
        justJumped = false;
    }
}
