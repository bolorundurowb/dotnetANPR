/*
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
 
using System.Xml.XPath;
using System.Xml;
using System.IO;

namespace dotNETANPR.Configurator
{
    class Properties
    {
        public string GetProperty(string propertyName)
        {
            try
            {
                XPathDocument doc = new XPathDocument("config.xml");
                XPathNavigator nav = doc.CreateNavigator();
                XPathExpression expr;
                expr = nav.Compile("properties/entry[@key='" + propertyName + "']");
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    return iterator.Current.Value;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void SetProperty(string propertyName, string value)
        {
            XmlTextReader reader = new XmlTextReader("config.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            reader.Close();
            XmlNode oldNode;
            XmlElement root = doc.DocumentElement;
            oldNode = root.SelectSingleNode("//entry[@key='" + propertyName + "']");
            oldNode.InnerText = value;
            doc.Save("config.xml");
        }

        public void StoreToXML(FileStream fs, string comment)
        {

        }

        public void LoadFromXML(FileStream fs)
        {

        }
    }
}
