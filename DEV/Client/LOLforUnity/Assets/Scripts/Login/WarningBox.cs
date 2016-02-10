using UnityEngine;
using System.Collections;
using Assets.Scripts.Login;
using UnityEngine.UI;

public class WarningBox : MonoBehaviour {

    [SerializeField]
    private Text contentText;

    private WarningResult result;

    /// <summary>
    /// 弹出提示窗并显示
    /// </summary>
    /// <param name="value"></param>
    public void Active(WarningModel value)
    {
        contentText.text = value.Value;
        result = value.Result;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭提示窗
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
        if (result != null)
            result();
    }
}
