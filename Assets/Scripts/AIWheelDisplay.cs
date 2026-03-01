using System.Diagnostics;
using TMPro;
using UnityEngine;

public class AIWheelDisplay : MonoBehaviour
{
    public TextMeshProUGUI[] words = new TextMeshProUGUI[8];

    public void SetWords(string[] newWords)
    {
        if (newWords.Length != 8)
        {
            UnityEngine.Debug.LogError("Must pass exactly 8 words!");
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            words[i].text = newWords[i];
        }
    }

    public string GetWords(int x)
    {
        return words[x + 1].text;
    }
}