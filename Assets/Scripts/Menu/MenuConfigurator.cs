using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuConfigurator : MonoBehaviour
{
    private static string path;
    public static UnityEvent Load = new UnityEvent();

    public void MakeEven(Slider slider) => slider.value = 
        Convert.ToInt32(slider.value) - ((Convert.ToInt32(slider.value) % 2 != 0) ? 1 : 0);
    public void MakeEven(InputField inputField) => inputField.text =
        (Convert.ToInt32(inputField.text) - ((Convert.ToInt32(inputField.text) % 2 != 0) ? 1 : 0)).ToString();

    public static void SaveValue(int lineIndex, string value)
    {
        List<String> lines = File.ReadAllLines(path).ToList();

        while(lines.Count - 1 < lineIndex) lines.Add("0");
        lines[lineIndex] = value;

        File.WriteAllLines(path, lines);
    }

    public static string LoadValue(int lineIndex)
    {
        List<String> lines = File.ReadAllLines(path).ToList();
        if (lines.Count - 1 < lineIndex) return "0";
        else return lines[lineIndex] == "" ? "0" : lines[lineIndex];
    }

    public void GoToGame()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        Directory.CreateDirectory(Application.streamingAssetsPath + "/ConfigurationFiles/");

        path = Application.streamingAssetsPath + "/ConfigurationFiles/" + "сonfig" + ".txt";

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "");
        }
        
        Load.Invoke();
    }
}
