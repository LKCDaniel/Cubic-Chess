using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UIManager_Local2P : MonoBehaviour
{
    public static UIManager_Local2P Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private GameManager_Local2P.GameState previousState;
    public GameObject pauseButton, undoButton, revolveButton, drawButton, shade, PawnPromotionPanel, Description;
    private string pawnPromotionType;

    void Start()
    {
        PawnPromotionPanel.SetActive(false);
    }

    void Update()
    {
        if (!BoardManager.Instance.inTransition)
        {
            pauseButton.SetActive(GameManager_Local2P.Instance.CurrentState == GameManager_Local2P.GameState.Running || GameManager_Local2P.Instance.CurrentState == GameManager_Local2P.GameState.Paused);
            undoButton.SetActive(GameManager_Local2P.Instance.CurrentState == GameManager_Local2P.GameState.Running && BoardManager.Instance.currentStep > 0);
            revolveButton.SetActive(GameManager_Local2P.Instance.CurrentState == GameManager_Local2P.GameState.Running);
            drawButton.SetActive(GameManager_Local2P.Instance.CurrentState == GameManager_Local2P.GameState.Running);
        }
        else
        {
            pauseButton.SetActive(false);
            undoButton.SetActive(false);
            revolveButton.SetActive(false);
            drawButton.SetActive(false);
        }
    }

    public void UndoOnClick() => BoardManager.Instance.Undo();

    public void PauseOnClick()
    {
        if (GameManager_Local2P.Instance.CurrentState == GameManager_Local2P.GameState.Paused)
        {
            Debug.Log("Resume game.");
            GameManager_Local2P.Instance.ChangeState(previousState);
            shade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.Log("Pause game.");
            previousState = GameManager_Local2P.Instance.CurrentState;
            GameManager_Local2P.Instance.ChangeState(GameManager_Local2P.GameState.Paused);
            shade.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        }
    }

    public void RevolveOnClick() { BoardManager.Instance.RevolveBoard(true); }

    public void DrawOnClick() => GameFinish(2);

    public void GameFinish(int outcome) // 0: Dark, 1: White, 2: Draw
    {
        SetShade(0.5f);
        Description.SetActive(true);
        if (outcome == 2)
            Description.GetComponent<TMP_Text>().text = "It's a draw!";
        else
            Description.GetComponent<TMP_Text>().text = outcome == 1 ? "White wins!" : "Black wins!";
        GameManager_Local2P.Instance.outcome = outcome;
        if (GameManager_Local2P.Instance.CurrentState != GameManager_Local2P.GameState.End)
            GameManager_Local2P.Instance.ChangeState(GameManager_Local2P.GameState.End);
    }

    public void SetShade(float alpha) => shade.GetComponent<Image>().color = new Color(0, 0, 0, alpha);

    public void FadeShade()
    {
        StartCoroutine(FadeOutShade(BoardManager.Instance.entryTime));

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
        Debug.Log($"Pawn promotion to {type}");
    }


}
