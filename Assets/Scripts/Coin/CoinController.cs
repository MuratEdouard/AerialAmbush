using GLTFast.Schema;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class CoinController : MonoBehaviour
{
    public float rotationSpeed = 1.0f;
    public AudioSource coinSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIController uiController = GameObject.FindWithTag("UIController").GetComponent<UIController>();
            uiController.CollectCoin();
            coinSound.Play();

            Collider coinCollider = GetComponent<Collider>();
            Renderer coinRenderer = GetComponent<Renderer>();
            coinCollider.enabled = false;
            coinRenderer.enabled = false;

            // Start the coroutine to wait for the sound to finish
            StartCoroutine(WaitForSoundToFinish());
        }
    }

    IEnumerator WaitForSoundToFinish()
    {
        // Wait until the sound is finished playing
        while (coinSound.isPlaying)
        {
            yield return null;  // Wait for the next frame
        }

        // Once the sound has finished, destroy the coin
        Destroy(gameObject);
    }
}
