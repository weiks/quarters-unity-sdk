// Decompiled with JetBrains decompiler
// Type: UnityEditor.iOS.Xcode.PlistElementBoolean
// Assembly: UnityEditor.iOS.Extensions.Xcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BE0D0667-EA91-4EFF-8038-E7F264A52EC5
// Assembly location: C:\Program Files\Unity5.6.0f1\Editor\Data\PlaybackEngines\iOSSupport\UnityEditor.iOS.Extensions.Xcode.dll

namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents a boolean element in plist document.</para>
    /// </summary>
    public class PlistElementBoolean : PlistElement
    {
        /// <summary>
        ///   <para>The value stored in the boolean element.</para>
        /// </summary>
        public bool value;

        /// <summary>
        ///   <para>Creates new boolean element.</para>
        /// </summary>
        /// <param name="v">The value of the element.</param>
        public PlistElementBoolean(bool v)
        {
            this.value = v;
        }
    }
}
