using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveLoader : MonoBehaviour
{
    public static void SaveValue(string path, int lineIndex, string value)
    {
        List<string> lines = File.ReadAllLines(path).ToList();

        while(lines.Count - 1 < lineIndex) lines.Add("0");
        lines[lineIndex] = value;

        File.WriteAllLines(path, lines);
    }

    public static string LoadValueNumber(string path, int lineIndex)
    {
        List<string> lines = File.ReadAllLines(path).ToList();
        if (lines.Count - 1 < lineIndex) return "0";
        else return lines[lineIndex] == "" ? "0" : lines[lineIndex];
    }
    
    public static string LoadValueString(string path, int lineIndex)
    {
        List<string> lines = File.ReadAllLines(path).ToList();
        if (lines.Count - 1 < lineIndex) return "";
        else return lines[lineIndex];
    }
}
