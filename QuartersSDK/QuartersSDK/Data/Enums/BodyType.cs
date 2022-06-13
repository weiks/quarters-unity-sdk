
using System.Runtime.Serialization;

namespace QuartersSDK.Data.Enums
{
    public enum BodyType
    {
        [EnumMember(Value = "application/json")]
        JSON,
        [EnumMember(Value = "form-data")]
        FORM,
    }
}
