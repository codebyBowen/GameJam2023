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
    private bool isClosedDialogue = false;

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
        yield return new WaitForSeconds(5);
        CloseDialogue();
    }

    public void DisplayDialog(int dialogueIndex) 
    {
        // trigger specific dialogue when event happens
        // index 0 is empty string
        textDisplay.text = "";
        Debug.Log("TriggerDisplayDialog" + dialogueIndex);
        if (dialogueIndex <= sentenses.Length - 1 && dialogueIndex > 0)
        {
            index = dialogueIndex;
            Background.SetActive(true);
            isClosedDialogue = false;
            StartCoroutine(Type());
        } else {
            Background.SetActive(false);
            index = 0;
        }
    }
    void CloseDialogue() 
    {
        if (!isClosedDialogue) {
            DisplayDialog(0);
            isClosedDialogue = true;
        }
    }
}
