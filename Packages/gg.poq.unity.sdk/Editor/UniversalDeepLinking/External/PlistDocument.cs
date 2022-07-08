using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.External
{
    /// <summary>
    ///   <para>Represents an Apple's plist document.</para>
    /// </summary>
    public class PlistDocument
    {
        /// <summary>
        ///   <para>The root element of the plist document.</para>
        /// </summary>
        public PlistElementDict root;
        /// <summary>
        ///   <para>The version of the plist document. At the moment Apple uses '1.0' for all plist files.</para>
        /// </summary>
        public string version;

        /// <summary>
        ///   <para>Creates a new plist document instance.</para>
        /// </summary>
        public PlistDocument()
        {
            this.root = new PlistElementDict();
            this.version = "1.0";
        }

        internal static XDocument ParseXmlNoDtd(string text)
        {
            return XDocument.Load(XmlReader.Create((TextReader)new StringReader(text), new XmlReaderSettings()
            {
                ProhibitDtd = false,
                XmlResolver = (XmlResolver)null
            }));
        }

        internal static string CleanDtdToString(XDocument doc)
        {
            if (doc.DocumentType != null)
            {
                XDocument xdocument = new XDocument(new XDeclaration("1.0", "utf-8", (string)null), new object[2]
                {
          (object) new XDocumentType(doc.DocumentType.Name, doc.DocumentType.PublicId, doc.DocumentType.SystemId, (string) null),
          (object) new XElement(doc.Root.Name)
                });
                return "" + (object)xdocument.Declaration + "\n" + (object)xdocument.DocumentType + "\n" + (object)doc.Root;
            }
            return "" + (object)new XDocument(new XDeclaration("1.0", "utf-8", (string)null), new object[1]
            {
        (object) new XElement(doc.Root.Name)
            }).Declaration + Environment.NewLine + (object)doc.Root;
        }

        private static string GetText(XElement xml)
        {
            return string.Join("", xml.Nodes().OfType<XText>().Select<XText, string>((Func<XText, string>)(x => x.Value)).ToArray<string>());
        }

        private static PlistElement ReadElement(XElement xml)
        {
            switch (xml.Name.LocalName)
            {
                case "dict":
                    List<XElement> list1 = xml.Elements().ToList<XElement>();
                    PlistElementDict plistElementDict = new PlistElementDict();
                    if (list1.Count % 2 == 1)
                        throw new Exception("Malformed plist file");
                    for (int index1 = 0; index1 < list1.Count - 1; ++index1)
                    {
                        if (list1[index1].Name != (XName)"key")
                            throw new Exception("Malformed plist file");
                        string index2 = PlistDocument.GetText(list1[index1]).Trim();
                        PlistElement plistElement = PlistDocument.ReadElement(list1[index1 + 1]);
                        if ((object)plistElement != null)
                        {
                            ++index1;
                            plistElementDict[index2] = plistElement;
                        }
                    }
                    return (PlistElement)plistElementDict;
                case "array":
                    List<XElement> list2 = xml.Elements().ToList<XElement>();
                    PlistElementArray plistElementArray = new PlistElementArray();
                    foreach (XElement xml1 in list2)
                    {
                        PlistElement plistElement = PlistDocument.ReadElement(xml1);
                        if ((object)plistElement != null)
                            plistElementArray.values.Add(plistElement);
                    }
                    return (PlistElement)plistElementArray;
                case "string":
                    return (PlistElement)new PlistElementString(PlistDocument.GetText(xml));
                case "integer":
                    int result;
                    if (int.TryParse(PlistDocument.GetText(xml), out result))
                        return (PlistElement)new PlistElementInteger(result);
                    return (PlistElement)null;
                case "true":
                    return (PlistElement)new PlistElementBoolean(true);
                case "false":
                    return (PlistElement)new PlistElementBoolean(false);
                default:
                    return (PlistElement)null;
            }
        }

        /// <summary>
        ///   <para>Reads the document from a file identified by the given path.</para>
        /// </summary>
        /// <param name="path">Path of the file.</param>
        public void ReadFromFile(string path)
        {
            this.ReadFromString(File.ReadAllText(path));
        }

        /// <summary>
        ///   <para>Reads the project from the given text reader.</para>
        /// </summary>
        /// <param name="tr">The project contents.</param>
        public void ReadFromStream(TextReader tr)
        {
            this.ReadFromString(tr.ReadToEnd());
        }

        /// <summary>
        ///   <para>Reads the document from the given string.</para>
        /// </summary>
        /// <param name="text">The project contents.</param>
        public void ReadFromString(string text)
        {
            XDocument xmlNoDtd = PlistDocument.ParseXmlNoDtd(text);
            this.version = (string)xmlNoDtd.Root.Attribute((XName)"version");
            PlistElement plistElement = PlistDocument.ReadElement(xmlNoDtd.XPathSelectElement("plist/dict"));
            if (plistElement == null)
                throw new Exception("Error parsing plist file");
            this.root = plistElement as PlistElementDict;
            if (this.root == null)
                throw new Exception("Malformed plist file");
        }

        private static XElement WriteElement(PlistElement el)
        {
            if (el is PlistElementBoolean)
                return new XElement((XName)(!(el as PlistElementBoolean).value ? "false" : "true"));
            if (el is PlistElementInteger)
                return new XElement((XName)"integer", (object)(el as PlistElementInteger).value.ToString());
            if (el is PlistElementString)
                return new XElement((XName)"string", (object)(el as PlistElementString).value);
            if (el is PlistElementDict)
            {
                PlistElementDict plistElementDict = el as PlistElementDict;
                XElement xelement1 = new XElement((XName)"dict");
                foreach (KeyValuePair<string, PlistElement> keyValuePair in (IEnumerable<KeyValuePair<string, PlistElement>>)plistElementDict.values)
                {
                    XElement xelement2 = new XElement((XName)"key", (object)keyValuePair.Key);
                    XElement xelement3 = PlistDocument.WriteElement(keyValuePair.Value);
                    if (xelement3 != null)
                    {
                        xelement1.Add((object)xelement2);
                        xelement1.Add((object)xelement3);
                    }
                }
                return xelement1;
            }
            if (!(el is PlistElementArray))
                return (XElement)null;
            PlistElementArray plistElementArray = el as PlistElementArray;
            XElement xelement4 = new XElement((XName)"array");
            foreach (PlistElement el1 in plistElementArray.values)
            {
                XElement xelement1 = PlistDocument.WriteElement(el1);
                if (xelement1 != null)
                    xelement4.Add((object)xelement1);
            }
            return xelement4;
        }

        /// <summary>
        ///   <para>Writes the project contents to the specified file.</para>
        /// </summary>
        /// <param name="path">Path to write the document contents to.</param>
        public void WriteToFile(string path)
        {
            File.WriteAllText(path, this.WriteToString());
        }

        /// <summary>
        ///   <para>Writes the document contents to the specified text writer.</para>
        /// </summary>
        /// <param name="tw">Text writer to write to.</param>
        public void WriteToStream(TextWriter tw)
        {
            tw.Write(this.WriteToString());
        }

        /// <summary>
        ///   <para>Writes the document contents to a string.</para>
        /// </summary>
        /// <returns>
        ///   <para>The project contents converted to string.</para>
        /// </returns>
        public string WriteToString()
        {
            XElement xelement1 = PlistDocument.WriteElement((PlistElement)this.root);
            XElement xelement2 = new XElement((XName)"plist");
            xelement2.Add((object)new XAttribute((XName)"version", (object)this.version));
            xelement2.Add((object)xelement1);
            XDocument doc = new XDocument();
            doc.Add((object)xelement2);
            return PlistDocument.CleanDtdToString(doc);
        }
    }
}
