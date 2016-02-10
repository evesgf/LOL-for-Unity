using UnityEngine;
using System.Collections;
using Assets.Scripts.Login;
using UnityEngine.UI;

// ReSharper disable CheckNamespace

public class LoginScene : MonoBehaviour
{
    #region 登录界面 杨定鹏 2015年9月21日14:19:25
    [SerializeField]
    private InputField accpuntInput;

    [SerializeField]
    private InputField passwordInput;
    #endregion

    [SerializeField]
    private Button loginBtn;

    [SerializeField]
    private GameObject regPanel;

    #region 注册界面 杨定鹏 2015年9月21日14:27:06

    [SerializeField] private InputField regAccountInput;

    [SerializeField] private InputField regPasswordInput;

    [SerializeField] private InputField regAgainPwdInput;
    #endregion

    /// <summary>
    /// 登录框点击事件
    /// </summary>
    public void LoginOnClick()
    {
        //数据合法性验证
        if (accpuntInput.text.Length == 0 || accpuntInput.text.Length < 6)
        {
            WarningCtl.Errors.Add(new WarningModel("账号不合法", delegate
            {
                loginBtn.interactable = true;
            }));
            Debug.Log("账号不合法");
            return;
        }
        if (passwordInput.text.Length == 0 || passwordInput.text.Length < 6)
        {
            WarningCtl.Errors.Add(new WarningModel("密码不合法"));
            Debug.Log("密码不合法");
            return;  
        }

        //验证通过 申请登录
        loginBtn.interactable = false;
    }

    /// <summary>
    /// 弹出注册窗口
    /// </summary>
    public void RegOpenClick()
    {
        regPanel.SetActive(true);
    }

    /// <summary>
    /// 关闭注册窗口
    /// </summary>
    public void RegCloseClick()
    {
        regPanel.SetActive(false);
    }

    /// <summary>
    /// 注册按钮点击事件
    /// </summary>
    public void RegClick()
    {
        if (regAccountInput.text.Length == 0 || regAccountInput.text.Length < 6)
        {
            WarningCtl.Errors.Add(new WarningModel("账号不合法"));
            Debug.Log("账号不合法");
        }
        if (regPasswordInput.text.Length == 0 || regPasswordInput.text.Length < 6)
        {
            WarningCtl.Errors.Add(new WarningModel("密码不合法"));
            Debug.Log("密码不合法");
        }
        if (regAgainPwdInput.text.Length == 0 || regAgainPwdInput.text.Length < 6)
        {
            WarningCtl.Errors.Add(new WarningModel("重复密码不合法"));
            Debug.Log("重复密码不合法");
        }

        //验证通过 申请注册 并关闭注册面板
    }
}
