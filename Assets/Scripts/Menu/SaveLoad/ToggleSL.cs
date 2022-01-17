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
        toggle.isOn = Convert.ToInt32(MenuConfigurator.LoadValue(lineIndex)) == 1;
    }

    public override void Save()
    {
        MenuConfigurator.SaveValue(lineIndex, (toggle.isOn ? 1 : 0).ToString());
    }
}
