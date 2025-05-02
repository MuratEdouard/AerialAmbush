using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class ProgressController: MonoBehaviour
{
    public UIDocument uiDocument;
    public Sprite[] sprites;
    public UnityEvent progressCompleted;

    private int nbWins = 0;
    private VisualElement root;

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
    }

    public void OnWin()
    {
        nbWins++;
        if (nbWins < sprites.Length)
        {
            var element = root.Q<VisualElement>("Image");
            element.style.backgroundImage = new StyleBackground(sprites[nbWins]);

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

            element.style.unityBackgroundImageTintColor = new StyleColor(tint);
        }
        else
        {
            progressCompleted.Invoke();
        }

    }
}
