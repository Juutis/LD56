using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TickingText : MonoBehaviour
{
    private float revealedCharacters;
    private string origText;
    private TextMeshProUGUI text;
    private float charsPerSecond = 25;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        origText = text.text;
    }

    // Update is called once per frame
    void Update()
    {
        var charsPerSecondForReal = charsPerSecond;
        if (Input.anyKey) {
            charsPerSecondForReal = charsPerSecond * 10;
        }
        revealedCharacters += Time.deltaTime * charsPerSecondForReal;

        int t = Mathf.Min((int)revealedCharacters, origText.Length); 
        var subtext = origText.Substring(0, t);
        text.SetText(subtext);
    }

    public bool IsDone() {
        return text.text.Length == origText.Length;
    }
}
