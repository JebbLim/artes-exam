using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUIController : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI ScoreText;

    private SC_GameLogic gameLogic;
    private GameConfig gameConfig;
    private float targetScore = 0.0f;
    private float displayScore = 0.0f;

    private Coroutine updateSequence = null;

    private void Start()
    {
        gameLogic = SC_GameLogic.Instance;
        gameConfig = GameConfig.Config;

        gameLogic.EvtScoreUpdated.AddListener(OnScoreUpdated);
        ScoreText.text = Format(0.0f);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnScoreUpdated(int _score)
    {
        targetScore = _score;

        if (updateSequence == null)
        {
            updateSequence = StartCoroutine(UpdateCO());
        }
    }

    private IEnumerator UpdateCO()
    {
        while (Mathf.Approximately(displayScore, targetScore) == false)
        {
            displayScore = Mathf.Lerp(displayScore, targetScore, gameConfig.ScoreUpdateSpeed * Time.deltaTime);
            ScoreText.text = Format(displayScore);
            yield return null;
        }

        displayScore = targetScore;
        ScoreText.text = Format(displayScore);

        updateSequence = null;
    }

    private string Format(float value)
    {
        return value.ToString(gameConfig.ValueStringFormat);
    }
}
