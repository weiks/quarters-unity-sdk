
using System.Collections.Generic;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents an array element in plist document.</para>
    /// </summary>
    public class PlistElementArray : PlistElement
    {
        /// <summary>
        ///   <para>The values stored in the array element.</para>
        /// </summary>
        public List<PlistElement> values = new List<PlistElement>();

        /// <summary>
        ///   <para>Convenience method to append new string element to values.</para>
        /// </summary>
        /// <param name="val">The value of the new string element.</param>
        public void AddString(string val)
        {
            this.values.Add((PlistElement)new PlistElementString(val));
        }

        /// <summary>
        ///   <para>Convenience method to append new integer element to values.</para>
        /// </summary>
        /// <param name="val">The value of the new integer element.</param>
        public void AddInteger(int val)
        {
            this.values.Add((PlistElement)new PlistElementInteger(val));
        }

        /// <summary>
        ///   <para>Convenience method to append new boolean element to values.</para>
        /// </summary>
        /// <param name="val">The value of the new boolean element.</param>
        public void AddBoolean(bool val)
        {
            this.values.Add((PlistElement)new PlistElementBoolean(val));
        }

        /// <summary>
        ///   <para>Convenience method to append new array to values.</para>
        /// </summary>
        /// <returns>
        ///   <para>The new array element.</para>
        /// </returns>
        public PlistElementArray AddArray()
        {
            PlistElementArray plistElementArray = new PlistElementArray();
            this.values.Add((PlistElement)plistElementArray);
            return plistElementArray;
        }

        /// <summary>
        ///   <para>Convenience method to append new dictionary to values.</para>
        /// </summary>
        /// <returns>
        ///   <para>The new dictionary element.</para>
        /// </returns>
        public PlistElementDict AddDict()
        {
            PlistElementDict plistElementDict = new PlistElementDict();
            this.values.Add((PlistElement)plistElementDict);
            return plistElementDict;
        }
    }
}
