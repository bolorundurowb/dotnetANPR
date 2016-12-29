using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace dotNETANPR.Configurator
{
    public class PropertyConfig : NameValueCollection
    {

        public void LoadFromXml(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The given file doesn't exist.");
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            foreach (XmlNode node in xmlDocument.DocumentElement.ChildNodes[0])
            {
                this[node.Name] = node.Value;
            }
        }

        public void StoreToXml(string filePath, string comment)
        {
            XmlDocument xmlDocument = new XmlDocument();
            var root = xmlDocument.CreateElement("config");
            // Insert Comment
            var xmlComment = xmlDocument.CreateComment(comment);
            root.AppendChild(xmlComment);
            foreach (KeyValuePair<string, string> pair in this)
            {
                var configItem = xmlDocument.CreateElement(pair.Key);
                configItem.Value = pair.Value;
                root.AppendChild(configItem);
            }
            xmlDocument.AppendChild(root);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            xmlDocument.Save(filePath);
        }
    }
}
