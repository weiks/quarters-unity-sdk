

namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents a plist element.</para>
    /// </summary>
    public class PlistElement
    {
        protected PlistElement()
        {
        }

        /// <summary>
        ///   <para>Convenience method to convert to string.</para>
        /// </summary>
        /// <returns>
        ///   <para>The value of the string element.</para>
        /// </returns>
        public string AsString()
        {
            return ((PlistElementString)this).value;
        }

        /// <summary>
        ///   <para>Convenience method to convert to integer.</para>
        /// </summary>
        /// <returns>
        ///   <para>The value of the integer element.</para>
        /// </returns>
        public int AsInteger()
        {
            return ((PlistElementInteger)this).value;
        }

        /// <summary>
        ///   <para>Convenience method to convert to bool.</para>
        /// </summary>
        /// <returns>
        ///   <para>The value of the boolean element.</para>
        /// </returns>
        public bool AsBoolean()
        {
            return ((PlistElementBoolean)this).value;
        }

        /// <summary>
        ///   <para>Convenience method to convert to array element.</para>
        /// </summary>
        /// <returns>
        ///   <para>The element as PlistElementArray.</para>
        /// </returns>
        public PlistElementArray AsArray()
        {
            return (PlistElementArray)this;
        }

        /// <summary>
        ///   <para>Convenience method to convert to dictionary element.</para>
        /// </summary>
        /// <returns>
        ///   <para>The element as PlistElementDict.</para>
        /// </returns>
        public PlistElementDict AsDict()
        {
            return (PlistElementDict)this;
        }

        public PlistElement this[string key]
        {
            get
            {
                return this.AsDict()[key];
            }
            set
            {
                this.AsDict()[key] = value;
            }
        }
    }
}
