using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSL : UISL
{
    private Toggle toggle;

    protected override void Init()
    {
        toggle = GetComponent<Toggle>();
    }

    protected override void Load()
    {
        toggle.isOn = Convert.ToInt32(SaveLoader.LoadValueNumber(MenuConfigurator.GetPath(), lineIndex)) == 1;
    }

    public override void Save()
    {
        SaveLoader.SaveValue(MenuConfigurator.GetPath(), lineIndex, (toggle.isOn ? 1 : 0).ToString());
    }
}
