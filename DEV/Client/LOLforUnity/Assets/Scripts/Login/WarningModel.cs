using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Login
{
    public delegate void WarningResult();
    public class WarningModel
    {
        public WarningResult Result;
        public string Value;

        public WarningModel(string value, WarningResult result =null)
        {
            Value = value;
            Result = result;
        }
    }
}