using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderSL : UISL
{
    private Slider slider;

    protected override void Init()
    {
        slider = GetComponent<Slider>();
    }

    protected override void Load()
    {
        slider.value = Convert.ToInt32(SaveLoader.LoadValueNumber(MenuConfigurator.GetPath(), lineIndex));
    }

    public override void Save()
    {
        SaveLoader.SaveValue(MenuConfigurator.GetPath(), lineIndex, slider.value.ToString());
    }
}
