using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


  public void OnStartGameClick()
    {
        SlingShot.isFirstShot = GameObject.Find("cb_Toggle").GetComponent<Toggle>().isOn;
        if(!SlingShot.isFirstShot)
        {
            SlingShot.firstName = GameObject.Find("txt_Enemy").GetComponent<InputField>().text;
            SlingShot.secondName= GameObject.Find("txt_Nickname").GetComponent<InputField>().text;
        }
        else
        {
            SlingShot.firstName = GameObject.Find("txt_Nickname").GetComponent<InputField>().text;
            SlingShot.secondName = GameObject.Find("txt_Enemy").GetComponent<InputField>().text;
        }
       
        SceneManager.LoadScene("Game");
    }
}
