using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class UIController : MonoBehaviour
{
    public UnityEvent countdownCompleted;
    public UnityEvent allCoinsCollected;
    public UnityEvent gameWon;

    private int nbCoinsCollected = 0;
    private int nbCountdown = 3;

    private VisualElement root;
    private Label countdownLabel;
    private Label winLabel;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        countdownLabel = root.Q<Label>("CountdownLabel");
        winLabel = root.Q<Label>("WinLabel");

        winLabel.style.display = DisplayStyle.None;

        StartCountdown();
    }

    void StartCountdown()
    {
        // Start the game begin countdown
        InvokeRepeating(nameof(DecreaseCountdown), 1f, 1f);
    }

    private void DecreaseCountdown()
    {
        nbCountdown--;

        if (nbCountdown > 0)
        {
            countdownLabel.text = nbCountdown.ToString();
        }
        else
        {
            countdownLabel.style.display = DisplayStyle.None;
            CancelInvoke(nameof(DecreaseCountdown));
            countdownCompleted.Invoke();
        }

    }

    public void CollectCoin()
    {
        List<VisualElement> coins = root.Query("Coin").ToList();

        coins[nbCoinsCollected].style.opacity = 1.0f;

        nbCoinsCollected++;

        // If all coins collected, send an event
        if (nbCoinsCollected == 5)
        {
            allCoinsCollected.Invoke();
        }
    }

    public void ShowWin()
    {
        winLabel.style.display = DisplayStyle.Flex;
        Invoke(nameof(GameWon), 3f);
    }

    public void GameWon()
    {
        gameWon.Invoke();
    }
}
