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
        inputField.text = MenuConfigurator.LoadValue(lineIndex);
    }

    public override void Save()
    {
        MenuConfigurator.SaveValue(lineIndex, inputField.text);
    }
}
