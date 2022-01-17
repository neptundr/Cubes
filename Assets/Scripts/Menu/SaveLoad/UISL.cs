using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UISL : MonoBehaviour
{
    [SerializeField] protected int lineIndex;
    
    private void Awake()
    {
        Init();
        MenuConfigurator.Load.AddListener(Load);
    }
    
    protected abstract void Init();
    protected abstract void Load();
    public abstract void Save();
}
