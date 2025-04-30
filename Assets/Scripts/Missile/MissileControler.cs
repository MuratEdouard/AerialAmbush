using UnityEngine;

public class MissileController : MonoBehaviour
{
    public float speed = 2.5f;
    public float steerForce = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;
    private Vector3 targetPosition = Vector3.zero;

    private bool isExploding = false;

    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private AudioSource missileAudio;
    [SerializeField] private GameObject meshObject;

    public void Start()
    {
        Invoke(nameof(Explode), 7f);

        // Add random rotation
        Vector3 randomRotation = new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f)
        );
        transform.Rotate(randomRotation);

        velocity = transform.forward * speed;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            targetPosition = player.transform.position;
    }

    void Update()
    {
        if (isExploding && !explosionParticles.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (isExploding) return;

        Seek();

        velocity += acceleration * Time.fixedDeltaTime;

        if (velocity.magnitude > speed)
            velocity = velocity.normalized * speed;

        transform.position += velocity * Time.fixedDeltaTime;

        // Rotate to face movement direction
        if (velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
    }

    private void Seek()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            targetPosition = player.transform.position;

            Vector3 desired = (targetPosition - transform.position).normalized * speed;
            Vector3 steer = (desired - velocity).normalized * steerForce;
            acceleration = steer;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assume Player has a method called Die()
            other.GetComponent<PlayerController>()?.Die();
            Explode();
        }
    }

    public void Explode()
    {
        if (isExploding) return;

        isExploding = true;
        acceleration = Vector3.zero;
        velocity = Vector3.zero;

        if (trailParticles != null) trailParticles.Stop();
        if (explosionParticles != null) explosionParticles.Play();
        if (missileAudio != null) missileAudio.Stop();
        if (meshObject != null) meshObject.SetActive(false);
    }
}
