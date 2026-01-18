using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;




public class ChatInputLegacy : MonoBehaviour
{
    public InputField inputField;
    public RawImage talkImage;
    public Text talkText;
    private String[] keyWords = new[] { "B10", "B9", "B8", "A4", "B4" };

    void Start()
    {
        StartCoroutine(ShowTalking("Hãy hỏi tui"));
        inputField.onSubmit.AddListener(HandleSubmit);
    }

    IEnumerator ShowTalking(String text)
    {
        Debug.Log(text);
        talkImage.gameObject.SetActive(true);
        talkText.text = text;
        yield return new WaitForSecondsRealtime(2f);
        talkImage.gameObject.SetActive(false);
    }

    void HandleSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            Debug.Log("Send: " + text);
            foreach (String word in keyWords)
            {
                if (text.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    StartCoroutine(ShowTalking(word));
                    GlobalProperties.Instance.IsShowNavigation = true;
                }
            }

            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

}