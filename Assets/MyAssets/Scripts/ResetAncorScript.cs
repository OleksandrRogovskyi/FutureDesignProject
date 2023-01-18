using System.Collections;
using System.Collections.Generic;
using Niantic.ARDKExamples;
using UnityEngine;
using UnityEngine.UI;

public class ResetAncorScript : MonoBehaviour
{
    public Button yourButton;

    void Start () {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick(){
        GameObject.Find("ExampleManager").GetComponent<ImageDetectionExampleManager>().ResetAncor();
    }

    
}
