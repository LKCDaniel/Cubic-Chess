using UnityEngine;

public class PattentAdvice : MonoBehaviour
{
    public GameObject LanguageDropdown;

    void Start(){
        LanguageDropdown.SetActive(false);
    }

    public void CloseOnClick(){
        LanguageDropdown.SetActive(true);
        gameObject.SetActive(false);
    }
}
