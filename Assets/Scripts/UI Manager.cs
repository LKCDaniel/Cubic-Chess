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

    [Header("UI Elements")]
    public GameObject pauseButton, undoButton;
    public GameObject shade;


    void Start()
    {

    }

    void Update()
    {
        if (!GameManager.Instance.inTransition && GameManager.Instance.CurrentState == GameManager.GameState.Running)
        {
            pauseButton.SetActive(true);
            undoButton.SetActive(GameManager.Instance.currentStep > 0);
        }
        else
        {
            pauseButton.SetActive(false);
            undoButton.SetActive(false);
        }
    }

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

    public void UndoOnClick()
    {
        GameManager.Instance.UndoStep();
    }

    public void GameFinish(bool isWhiteWin)
    {

    }

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

    private void SetShade(float alpha)
    {
        shade.GetComponent<Image>().color = new Color(0, 0, 0, alpha);
    }


}
