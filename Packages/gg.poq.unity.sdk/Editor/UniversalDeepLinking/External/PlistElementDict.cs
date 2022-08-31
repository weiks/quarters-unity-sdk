
using System.Collections.Generic;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents a dictionary element in plist document.</para>
    /// </summary>
    public class PlistElementDict : PlistElement
    {
        private SortedDictionary<string, PlistElement> m_PrivateValue = new SortedDictionary<string, PlistElement>();

        /// <summary>
        ///   <para>The values stored in the dictionary element.</para>
        /// </summary>
        public IDictionary<string, PlistElement> values
        {
            get
            {
                return (IDictionary<string, PlistElement>)this.m_PrivateValue;
            }
        }

        public new PlistElement this[string key]
        {
            get
            {
                if (this.values.ContainsKey(key))
                    return this.values[key];
                return (PlistElement)null;
            }
            set
            {
                this.values[key] = value;
            }
        }

        /// <summary>
        ///   <para>Convenience method to set an integer property.</para>
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="val">The value of the property.</param>
        public void SetInteger(string key, int val)
        {
            this.values[key] = (PlistElement)new PlistElementInteger(val);
        }

        /// <summary>
        ///   <para>Convenience method to set a string property.</para>
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="val">The value of the property.</param>
        public void SetString(string key, string val)
        {
            this.values[key] = (PlistElement)new PlistElementString(val);
        }

        /// <summary>
        ///   <para>Convenience method to set a boolean property.</para>
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="val">The value of the property.</param>
        public void SetBoolean(string key, bool val)
        {
            this.values[key] = (PlistElement)new PlistElementBoolean(val);
        }

        /// <summary>
        ///   <para>Convenience method to set a property to a new array element.</para>
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <returns>
        ///   <para>The new array element.</para>
        /// </returns>
        public PlistElementArray CreateArray(string key)
        {
            PlistElementArray plistElementArray = new PlistElementArray();
            this.values[key] = (PlistElement)plistElementArray;
            return plistElementArray;
        }

        /// <summary>
        ///   <para>Convenience method to set a property to a new dictionary element.</para>
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <returns>
        ///   <para>The new dictionary element.</para>
        /// </returns>
        public PlistElementDict CreateDict(string key)
        {
            PlistElementDict plistElementDict = new PlistElementDict();
            this.values[key] = (PlistElement)plistElementDict;
            return plistElementDict;
        }
    }
}
