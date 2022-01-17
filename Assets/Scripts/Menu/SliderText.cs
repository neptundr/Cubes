using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
        UpdateText();
    }

    public void UpdateText()
    {
        try{text.text = slider.value.ToString();}
        catch{Debug.Log(gameObject.name);}
    }
}
