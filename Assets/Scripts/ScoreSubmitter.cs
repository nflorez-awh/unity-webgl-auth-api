using TMPro;
using UnityEngine;

public class ScoreSubmitter : MonoBehaviour
{
    [SerializeField] private TMP_InputField scoreInput;

    public void SubmitButtonHandler()
    {
        if (string.IsNullOrEmpty(scoreInput.text)) return;

        if (int.TryParse(scoreInput.text, out int score))
        {
            FindObjectOfType<AuthHandler>().SubmitScore(score);
            scoreInput.text = "";
        }
    }
}