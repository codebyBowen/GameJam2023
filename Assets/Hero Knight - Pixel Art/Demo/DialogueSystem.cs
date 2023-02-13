using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public TMP_Text textDisplay;
    public string[] sentenses;
    private int index;
    public float typeSpeed;
    public GameObject Background; 

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Type());
        Background.SetActive(false);
    }

    IEnumerator Type() 
    {
        foreach(char letter in sentenses[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void DisplayDialog(int dialogueIndex) 
    {
        // trigger specific dialogue when event happens
        // index 0 is empty string
        textDisplay.text = "";
        Debug.Log("TriggerDisplayDialog" + sentenses[dialogueIndex]);
        if (dialogueIndex <= sentenses.Length - 1)
        {
            index = dialogueIndex;
            Background.SetActive(true);
            StartCoroutine(Type());
        } else {
            Background.SetActive(false);
            index = 0;
        }
    }
}
