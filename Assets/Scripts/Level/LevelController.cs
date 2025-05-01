using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [Header("Input")]
    public InputAction escInput;

    [Header("Spawns")]
    public GameObject missiles;
    public GameObject missilePrefab;

    public GameObject coins;
    public GameObject coinPrefab;

    public float missileSpawnTimer = 4f;

    private int nbCoinsToSpawn = 5;
    private UIController uiController;
    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Enable esc input
        escInput.performed += OnEscPressed;
        escInput.Enable();

        uiController = FindFirstObjectByType<UIController>();
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Initiate random with unique seed
        Random.InitState(System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000);

        // Place clouds in a random formation, with cloud center staying in the same spot
        GameObject[] clouds = GameObject.FindGameObjectsWithTag("Cloud");
        foreach (var cloud in clouds)
        {
            if (cloud.name.ToLower().Contains("center"))
                continue;

            int randY = Random.Range(1, 3);
            Vector3 newCloudPosition = new Vector3(cloud.transform.localPosition.x, randY, cloud.transform.localPosition.z);
            cloud.transform.localPosition = newCloudPosition;
        }

        // Spawn all coins to be collected
        int nbCoinsSpawned = 0;
        int i = 0;
        while(nbCoinsSpawned < nbCoinsToSpawn)
        {
            GameObject cloud = clouds[i];
            i++;

            // Do not spawn a coin at the center, directly where the player starts
            if (clouds[i].name.ToLower().Contains("center"))
                continue;

            Vector3 worldPosition = clouds[i].gameObject.transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            SpawnCoin(worldPosition);

            nbCoinsSpawned++;
        }
    }

    public void BeginGame()
    {
        // Spawn missiles at regular intervals
        InvokeRepeating(nameof(SpawnMissile), 0f, missileSpawnTimer);

        // Unfreeze the player
        playerController.Unfreeze();
    }

    private void SpawnMissile()
    {
        var rand_x = Random.Range(-10.0f, 10.0f);
        var rand_z = Random.Range(-10.0f, 10.0f);
        var rand_y = Random.Range(-1, 3);
        var rand_translation = new Vector3(rand_x, rand_y, rand_z);

        GameObject missile = Instantiate(missilePrefab, missiles.transform);
        missile.transform.localPosition = rand_translation;

    }

    private void SpawnCoin(Vector3 worldPosition)
    {
        GameObject coin = Instantiate(coinPrefab, coins.transform);
        coin.transform.position = worldPosition;
    }

    public void Win()
    {
        uiController.ShowWin();
        ExplodeAllMissiles();

        // Stop player from moving
        playerController.OnWin();

        // Restart Game after some time
        Invoke(nameof(RestartGame), 3.0f);
    }

    public void Lose()
    {
        uiController.ShowLose();
        ExplodeAllMissiles();

        // Stop player from moving
        playerController.OnLose();

        // Restart Game after some time
        Invoke(nameof(RestartGame), 3.0f);
    }

    private void ExplodeAllMissiles()
    {
        // Stop additional missiles from getting spawned
        CancelInvoke(nameof(SpawnMissile));

        // Explode all missiles
        MissileController[] missileControllers = FindObjectsByType<MissileController>(FindObjectsSortMode.None);
        foreach (var missileController in missileControllers)
        {
            missileController.Explode();
        }
    }

    private void RestartGame()
    {
        // Reload the active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnEscPressed(InputAction.CallbackContext context)
    {
        // Disable input
        escInput.performed -= OnEscPressed;
        escInput.Disable();

        // Destroy MusicController
        MusicController musicController = FindFirstObjectByType<MusicController>();
        Destroy(musicController.gameObject);

        // Load the menu scene
        SceneManager.LoadScene("Menu");
    }
}
