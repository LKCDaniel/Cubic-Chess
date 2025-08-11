using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager_Robot : MonoBehaviour
{
    public static UIManager_Robot Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private GameManager_Robot.GameState previousState;
    public GameObject pauseButton, undoButton, revolveButton, drawButton, exitButton, shade, PawnPromotionPanel, Description;
    private string pawnPromotionType;

    void Start()
    {
        PawnPromotionPanel.SetActive(false);
        exitButton.SetActive(false);
    }

    void Update()
    {
        if (!BoardManager.Instance.inTransition)
        {
            pauseButton.SetActive(GameManager_Robot.Instance.CurrentState == GameManager_Robot.GameState.PlayerTurn || GameManager_Robot.Instance.CurrentState == GameManager_Robot.GameState.Paused);
            undoButton.SetActive(GameManager_Robot.Instance.CurrentState == GameManager_Robot.GameState.PlayerTurn && BoardManager.Instance.currentStep > 1);
            revolveButton.SetActive(GameManager_Robot.Instance.CurrentState == GameManager_Robot.GameState.PlayerTurn);
            drawButton.SetActive(GameManager_Robot.Instance.CurrentState == GameManager_Robot.GameState.PlayerTurn);
        }
        else
        {
            pauseButton.SetActive(false);
            undoButton.SetActive(false);
            revolveButton.SetActive(false);
            drawButton.SetActive(false);
        }
    }

    public void UndoOnClick() => BoardManager.Instance.Undo(2);

    public void PauseOnClick()
    {
        if (GameManager_Robot.Instance.CurrentState == GameManager_Robot.GameState.Paused)
        {
            Debug.Log("Resume game.");
            exitButton.SetActive(false);
            GameManager_Robot.Instance.ChangeState(previousState);
            shade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.Log("Pause game.");
            exitButton.SetActive(true);
            previousState = GameManager_Robot.Instance.CurrentState;
            GameManager_Robot.Instance.ChangeState(GameManager_Robot.GameState.Paused);
            shade.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        }
    }

    public void RevolveOnClick() { BoardManager.Instance.RevolveBoard(true); }

    public void DrawOnClick() => GameFinish(2);

    public void ExitOnClick() => SceneManager.LoadScene("Welcome");

    public void GameFinish(int outcome) // 0: Dark, 1: White, 2: Draw
    {
        SetShade(0.5f);
        Description.SetActive(true);
        if (outcome == 2)
            Description.GetComponent<TMP_Text>().text = "It's a draw!";
        else
            Description.GetComponent<TMP_Text>().text = outcome == 1 ? "White wins!" : "Black wins!";
        GameManager_Robot.Instance.outcome = outcome;
        if (GameManager_Robot.Instance.CurrentState != GameManager_Robot.GameState.End)
            GameManager_Robot.Instance.ChangeState(GameManager_Robot.GameState.End);
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
