using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.XPath;
using System.Xml;

namespace dotNETANPR.Configurator
{
    public class Properties
    {
        public string getProperty(string propertyName)
        {
            try
            {
                //settingsFilePath is a string variable storing the path of the settings file 
                XPathDocument doc = new XPathDocument("config.xml");
                XPathNavigator nav = doc.CreateNavigator();
                // Compile a standard XPath expression
                XPathExpression expr;
                expr = nav.Compile("properties/entry[@key='" + propertyName + "']");
                XPathNodeIterator iterator = nav.Select(expr);
                // Iterate on the node set
                while (iterator.MoveNext())
                {
                    return iterator.Current.Value;
                }
                return string.Empty;
            }
            catch
            {
                //do some error logging here. Leaving for you to do 
                return string.Empty;
            }
        }

        public void setProperty(string propertyName, string value)
        {
            //settingsFilePath is a string variable storing the path of the settings file 
            XmlTextReader reader = new XmlTextReader("config.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            //we have loaded the XML, so it's time to close the reader.
            reader.Close();
            XmlNode oldNode;
            XmlElement root = doc.DocumentElement;
            oldNode = root.SelectSingleNode("//entry[@key='" + propertyName + "']");
            oldNode.InnerText = value;
            doc.Save("config.xml");
        }

        public void storeToXML (FileStream fs, string comment)
        {

        }

        public void loadFromXML (FileStream fs)
        {

        }
    }
}
