using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public static UnityEvent Load = new UnityEvent();
    
    public static string GetPath() => path;
    private static string path;

    [SerializeField] private int colorPickerIndex;

    private Image image;

    public int GetColorPickerIndex() => colorPickerIndex;
    
    public void UpdateFiles(int colorChannelIndex, float value)
    {
        SaveLoader.SaveValue(path, colorPickerIndex * 3 + colorChannelIndex, value.ToString());
        LoadColor();
    }

    private void LoadColor()
    {
        image.color = new Color((float) Convert.ToDouble(SaveLoader.LoadValueNumber(path, colorPickerIndex * 3)),
            (float) Convert.ToDouble(SaveLoader.LoadValueNumber(path, colorPickerIndex * 3 + 1)),
            (float) Convert.ToDouble(SaveLoader.LoadValueNumber(path, colorPickerIndex * 3 + 2)));
    }

    private void Start()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + "/Colors/");

        path = Application.streamingAssetsPath + "/Colors/" + "colors" + ".txt";
        
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "0");
        }

        image = GetComponent<Image>();

        LoadColor();
        Load.Invoke();
    }
}

// public enum ColorChannel
// {
//     R,
//     G,
//     B
// }