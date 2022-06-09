using System;
using System.Collections.Generic;
using System.Text;

namespace QuartersSDK.Data.Interfaces
{
    interface ISerializable
    {
        public string ToJSONString();
    }
}
