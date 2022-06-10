
using System.Runtime.Serialization;

namespace QuartersSDK.Data.Enums
{
    public enum GrantType
    {
        [EnumMember(Value = "refresh_token")]
        REFRESH_TOKEN,
        [EnumMember(Value = "authorization_code")]
        AUTHORIZATION_CODE,
    }
}
