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

    // Update is called once per frame
    // void Update()
    // {
        
    // }

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
        Debug.Log("TriggerDisplayDialog" + dialogueIndex + sentenses.Length);
        if (dialogueIndex <= sentenses.Length - 1)
        {
            index = dialogueIndex;
            Background.SetActive(true);
        } else {
            Background.SetActive(false);
            index = 0;
        }
    }
}
