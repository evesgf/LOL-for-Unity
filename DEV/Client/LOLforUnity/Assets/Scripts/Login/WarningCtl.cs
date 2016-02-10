using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Login;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public class WarningCtl : MonoBehaviour
{
    public static  List<WarningModel> Errors=new List<WarningModel>();

    [SerializeField] private WarningBox warningBox;

    private void Update()
    {
        if (Errors.Count > 0)
        {
            WarningModel err = Errors[0];
            Errors.RemoveAt(0);
            warningBox.Active(err);
        }
    }
}
