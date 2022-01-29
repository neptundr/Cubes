using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderColorSetter : MonoBehaviour
{
    [SerializeField] private ColorPicker colorPicker; 
    [SerializeField] private int colorChannelIndex; 
    [SerializeField] private Slider slider;

    private bool justStarted = true;
    
    private void Awake()
    {
        ColorPicker.Load.AddListener(Load);
    }
    
    public void UpdateValue()
    {
        if (!justStarted) colorPicker.UpdateFiles(colorChannelIndex, slider.value);
        else justStarted = false;
    }

    private void Load()
    {
        slider.value = (float) Convert.ToDouble(SaveLoader.LoadValueNumber(ColorPicker.GetPath(),
                colorPicker.GetColorPickerIndex() * 3 + colorChannelIndex));
    }
}
