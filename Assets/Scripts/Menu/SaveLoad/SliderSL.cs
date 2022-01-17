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
        slider.value = Convert.ToInt32(MenuConfigurator.LoadValue(lineIndex));
    }

    public override void Save()
    {
        MenuConfigurator.SaveValue(lineIndex, slider.value.ToString());
    }
}
