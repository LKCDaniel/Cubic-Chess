using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private GameManager.GameState previousState;
    public GameObject pauseButton, undoButton, shade, PawnPromotionPanel;
    private string pawnPromotionType;

    void Start()
    {
        PawnPromotionPanel.SetActive(false);
    }

    void Update()
    {
        if (!GameManager.Instance.inTransition)
        {
            pauseButton.SetActive(GameManager.Instance.CurrentState == GameManager.GameState.Running || GameManager.Instance.CurrentState == GameManager.GameState.Paused);
            undoButton.SetActive(GameManager.Instance.CurrentState == GameManager.GameState.Running && GameManager.Instance.currentStep > 0);
        }
        else
        {
            pauseButton.SetActive(false);
            undoButton.SetActive(false);
        }
    }

    public void UndoOnClick() => GameManager.Instance.UndoStep();

    public void PauseOnClick()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
        {
            Debug.Log("Resume game.");
            GameManager.Instance.ChangeState(previousState);
            shade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.Log("Pause game.");
            previousState = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
            shade.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        }
    }

    public void GameFinish(bool isWhiteWin)
    {
        SetShade(0.5f);

    }

    public void SetShade(float alpha) => shade.GetComponent<Image>().color = new Color(0, 0, 0, alpha);

    public void FadeShade()
    {
        StartCoroutine(FadeOutShade(GameManager.Instance.entryTime));

        IEnumerator FadeOutShade(float duration)
        {
            float elapsedTime = 0;
            Color initialColor = Color.black;
            Color targetColor = new Color(0, 0, 0, 0);

            while (elapsedTime < duration)
            {
                shade.GetComponent<Image>().color = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            SetShade(0);
        }
    }

    public IEnumerator SetPromotionTypeCoroutine(System.Action<string> onComplete)
    {
        pawnPromotionType = null;
        PawnPromotionPanel.SetActive(true);
        yield return new WaitUntil(() => pawnPromotionType != null);
        PawnPromotionPanel.SetActive(false);
        onComplete?.Invoke(pawnPromotionType);
    }

    public void SetPromotionTypeOnClick(string type)
    {
        pawnPromotionType = type;
        Debug.Log($"Pawn promotion to {type} {(GameManager.Instance.isWhiteTurn ? "White" : "Dark")}.");
    }


}
