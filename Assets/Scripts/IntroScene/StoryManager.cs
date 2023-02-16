using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    public TMP_Text textDisplay;
    public string[] sentenses;
    public float typeSpeed;
    private int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Type());
    }
 
    IEnumerator Type() 
    {
        if (index <= sentenses.Length - 1) {
            textDisplay.text = "";
            foreach(char letter in sentenses[index].ToCharArray()) { 
                textDisplay.text += letter;
                yield return new WaitForSeconds(typeSpeed); 
            } 
            index +=1;
            yield return new WaitForSeconds(3);
            StartCoroutine(Type());
        } else {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
        }
    }
}
