using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Input")]
    public InputAction upArrowInput;
    public InputAction downArrowInput;
    public InputAction enterInput;

    [Header("Camera Setup")]
    public GameObject cameraPivot;
    public GameObject cameraArm;
    public GameObject mainCamera;
    public float pivotRotationSpeed = 10.0f;
    public float timeToZoom = 1.0f;
    public Vector3 cameraArmZoomPosition= new Vector3(0.0f, 0.3f, 1.2f);

    [Header("UI Elements")]
    public GameObject playTextObject;
    public GameObject quitTextObject;

    [Header("Audio")]
    public AudioSource menuOptionChangedAudio;
    public AudioSource menuOptionSelectedAudio;


    private TextMeshPro playText;
    private TextMeshPro quitText;

    private Color selectedTextColor = new Color(1.0f, 0.55f, 0f);
    private Color unselectedTextColor = new Color(0.4f, 0.22f, 0f);

    private bool playSelected = true;
    private bool quitSelected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Setup key bindings
        upArrowInput.performed += OnUpArrowPressed;
        downArrowInput.performed += OnDownArrowPressed;
        enterInput.performed += OnEnterPressed;

        upArrowInput.Enable();
        downArrowInput.Enable();
        enterInput.Enable();

        // Setup text variables
        playText = playTextObject.GetComponent<TextMeshPro>();
        quitText = quitTextObject.GetComponent<TextMeshPro>();

        // Prepare camera zoom in
        Invoke(nameof(ZoomInCamera), 1.0f);
    }

    void ZoomInCamera()
    {
        // Move camera in toward the player
        LeanTween.moveLocalX(cameraArm, cameraArmZoomPosition.x, timeToZoom).setEase(LeanTweenType.easeOutQuad);
        LeanTween.moveLocalY(cameraArm, cameraArmZoomPosition.y, timeToZoom).setEase(LeanTweenType.easeOutQuad);
        LeanTween.moveLocalZ(cameraArm, cameraArmZoomPosition.z, timeToZoom).setEase(LeanTweenType.easeOutQuad);
    }

    // Update is called once per frame
    void Update()
    {
        cameraPivot.transform.Rotate(Vector3.up, Time.deltaTime * pivotRotationSpeed);
    }

    private void OnUpArrowPressed(InputAction.CallbackContext context)
    {
        playSelected = true;
        quitSelected = false;

        playText.color = selectedTextColor;
        quitText.color = unselectedTextColor;
        
        menuOptionChangedAudio.Play();
    }

    private void OnDownArrowPressed(InputAction.CallbackContext context)
    {
        playSelected = false;
        quitSelected = true;

        playText.color = unselectedTextColor;
        quitText.color = selectedTextColor;
        
        menuOptionChangedAudio.Play();
    }

    private void OnEnterPressed(InputAction.CallbackContext context)
    {
        menuOptionSelectedAudio.Play();
        if (playSelected)
        {
            ChangeSceneToMain();
        } else
        {
            Application.Quit();
        }
    }

    private void ChangeSceneToMain()
    {
        // Disable inputs
        upArrowInput.performed -= OnUpArrowPressed;
        downArrowInput.performed -= OnDownArrowPressed;
        enterInput.performed -= OnEnterPressed;

        upArrowInput.Disable();
        downArrowInput.Disable();
        enterInput.Disable();

        // Load the new scene
        SceneManager.LoadScene("Main");
    }
}
