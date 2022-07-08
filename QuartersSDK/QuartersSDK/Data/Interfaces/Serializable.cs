namespace QuartersSDK.Data.Interfaces
{
    public abstract class Serializable : ISerializable
    {
        public string ToJSONString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}