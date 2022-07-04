namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents a string element in plist document.</para>
    /// </summary>
    public class PlistElementString : PlistElement
    {
        /// <summary>
        ///   <para>The value stored in the string element.</para>
        /// </summary>
        public string value;

        /// <summary>
        ///   <para>Creates new string element.</para>
        /// </summary>
        /// <param name="v">The value of the element.</param>
        public PlistElementString(string v)
        {
            this.value = v;
        }
    }
}
