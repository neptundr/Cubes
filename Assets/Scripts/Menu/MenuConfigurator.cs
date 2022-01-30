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

    [SerializeField] private GameObject localGroup;
    [SerializeField] private GameObject networkGroup;
    [SerializeField] private GameObject colorGroup;
    [SerializeField] private GameObject mapConfigurationGroup;
    
    public void MakeEven(Slider slider) => slider.value = 
        Convert.ToInt32(slider.value) - ((Convert.ToInt32(slider.value) % 2 != 0) ? 1 : 0);
    public void MakeEven(InputField inputField) => inputField.text =
        (Convert.ToInt32(inputField.text) - ((Convert.ToInt32(inputField.text) % 2 != 0) ? 1 : 0)).ToString();

    public static string GetPath() => path;

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

    public void SetLocal()
    {
        localGroup.SetActive(true);
        networkGroup.SetActive(false);
        colorGroup.SetActive(false);
    }

    public void SetNetwork()
    {
        localGroup.SetActive(false);
        networkGroup.SetActive(true);
        colorGroup.SetActive(false);
    }

    public void SetColors()
    {
        localGroup.SetActive(false);
        networkGroup.SetActive(false);
        colorGroup.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}