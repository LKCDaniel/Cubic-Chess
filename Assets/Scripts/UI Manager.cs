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
        Instance = this;
    }

    private GameManager.GameState previousState;

    [Header("UI Elements")]
    public GameObject pauseButton;
    public GameObject shade;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PauseOnClick()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            GameManager.Instance.ChangeState(previousState);
        else
        {
            previousState = GameManager.Instance.CurrentState;
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
        }
    }

    private void SetShade(float alpha)
    {
        shade.GetComponent<Image>().color = new Color(0, 0, 0, alpha);
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


}
