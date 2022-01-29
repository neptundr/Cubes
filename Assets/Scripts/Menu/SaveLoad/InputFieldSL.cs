using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldSL : UISL
{
    private InputField inputField;

    protected override void Init()
    {
        inputField = GetComponent<InputField>();
    }

    protected override void Load()
    {
        inputField.text = SaveLoader.LoadValueNumber(MenuConfigurator.GetPath(), lineIndex);
    }

    public override void Save()
    {
        SaveLoader.SaveValue(MenuConfigurator.GetPath(), lineIndex, inputField.text);
    }
}
