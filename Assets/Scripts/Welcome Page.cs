using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using UnityEngine.Localization;

public class WelcomePage : MonoBehaviour
{
    public GameObject playWhiteToggle, difficultyDropdown, languageDropdownObject;
    private TMP_Dropdown languageDropdown;
    private AsyncOperationHandle m_InitializeOperation;


    void OnEnable()
    {
        playWhiteToggle.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("PlayerWhite") == 1;
        difficultyDropdown.GetComponent<TMP_Dropdown>().value = PlayerPrefs.GetInt("RobotLevel");
        playWhiteToggle.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
        difficultyDropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnDifficultyChanged);
        languageDropdown = languageDropdownObject.GetComponent<TMP_Dropdown>();
    }

    void Start()
    {
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        languageDropdown.ClearOptions();
        languageDropdown.options.Add(new TMP_Dropdown.OptionData("Loading..."));
        languageDropdown.interactable = false;

        m_InitializeOperation = LocalizationSettings.SelectedLocaleAsync;
        if (m_InitializeOperation.IsDone)
            SetupLanguageOptions(m_InitializeOperation);
        else
            m_InitializeOperation.Completed += SetupLanguageOptions;
    }

    private void SetupLanguageOptions(AsyncOperationHandle obj)
    {
        var options = new List<string>();
        var locales = LocalizationSettings.AvailableLocales.Locales;
        int savedIndex = PlayerPrefs.GetInt("Language", 0);

        if (savedIndex < 0 || savedIndex >= locales.Count)
            savedIndex = 0;

        for (int i = 0; i < locales.Count; ++i)
        {
            var locale = locales[i];
            var displayName = locale.Identifier.CultureInfo?.NativeName ?? locale.name;
            options.Add(displayName);
        }

        if (options.Count == 0)
        {
            options.Add("No Locales Available");
            languageDropdown.interactable = false;
        }
        else
        {
            languageDropdown.interactable = true;
            LocalizationSettings.SelectedLocale = locales[savedIndex];
        }

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);
        languageDropdown.SetValueWithoutNotify(savedIndex);
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLanguageChanged(int index)
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        PlayerPrefs.SetInt("Language", index);
        PlayerPrefs.Save();
        Debug.Log($"Language changed to: {LocalizationSettings.SelectedLocale}");
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        int index = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
        languageDropdown.SetValueWithoutNotify(index);
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
