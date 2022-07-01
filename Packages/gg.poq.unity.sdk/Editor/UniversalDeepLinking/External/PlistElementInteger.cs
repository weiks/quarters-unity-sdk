namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents an integer element in plist document.</para>
    /// </summary>
    public class PlistElementInteger : PlistElement
    {
        /// <summary>
        ///   <para>The value stored in the integer element.</para>
        /// </summary>
        public int value;

        /// <summary>
        ///   <para>Creates new integer element.</para>
        /// </summary>
        /// <param name="v">The value of the element.</param>
        public PlistElementInteger(int v)
        {
            this.value = v;
        }
    }
}
