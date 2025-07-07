using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WelcomePage : MonoBehaviour
{
    public GameObject playWhiteToggle, difficultyDropdown;

    void OnEnable()
    {
        playWhiteToggle.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("PlayerWhite") == 1;
        difficultyDropdown.GetComponent<TMP_Dropdown>().value = PlayerPrefs.GetInt("RobotLevel");
        playWhiteToggle.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
        difficultyDropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnDifficultyChanged);
    }

    public void StartLocal2P()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Local2P");
    }

    public void StartRobot()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Robot");
    }

    private void OnValueChanged(bool isOn)
    {
        PlayerPrefs.SetInt("PlayerWhite", isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Player Color: {(isOn ? "White" : "Dark")}");
    }

    private void OnDifficultyChanged(int value)
    {
        PlayerPrefs.SetInt("RobotLevel", value);
        PlayerPrefs.Save();
        string difficulty = value switch
        {
            0 => "Easy",
            1 => "Medium",
            2 => "Hard",
            _ => "Unknown"
        };
        Debug.Log($"Robot Level: {difficulty}");
    }

}
