using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using static UnityEditor.Rendering.FilterWindow;

public class ProgressController: MonoBehaviour
{
    public UIDocument uiDocument;
    public Sprite[] sprites;

    private int nbWins = 0;
    private VisualElement root;
    private VisualElement image;

    public static ProgressController instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        root = uiDocument.rootVisualElement;
        image = root.Q<VisualElement>("Image");
    }

    public void OnWin()
    {
        nbWins++;
        
        AddGrowClass();
        Invoke(nameof(UpdateImageAndColor), 1f);
        Invoke(nameof(RemoveGrowClass), 2f);

        if (nbWins == sprites.Length - 1)
            Invoke(nameof(ProgressCompleted), 3f);
        else
            Invoke(nameof(RestartGameProxy), 3f);
    }

    private void AddGrowClass()
    {
        image.AddToClassList("grown");
    }

    private void RemoveGrowClass()
    {
        image.RemoveFromClassList("grown");
    }

    private void UpdateImageAndColor()
    {
        // Change progress image
        image.style.backgroundImage = new StyleBackground(sprites[nbWins]);

        // progress ratio between 0 and 1
        float t = (float)nbWins / (sprites.Length - 1);

        // Red to Yellow to Green: use two-phase Lerp
        Color tint;
        if (t < 0.5f)
        {
            tint = Color.Lerp(Color.red, Color.yellow, t * 2); // 0–0.5 = red to yellow
        }
        else
        {
            tint = Color.Lerp(Color.yellow, Color.green, (t - 0.5f) * 2); // 0.5–1 = yellow to green
        }

        image.style.unityBackgroundImageTintColor = new StyleColor(tint);
    }

    private void RestartGameProxy()
    {
        LevelController levelController = FindFirstObjectByType<LevelController>();
        levelController.RestartGame();
    }

    private void ProgressCompleted()
    {
        UIController uiController = FindFirstObjectByType<UIController>();
        uiController.ShowWin();
    }
}
