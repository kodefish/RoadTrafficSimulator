using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RoadTrafficSimulator.WorldData
{
    struct IntersectionInfo
    {
        public String id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }

    class GeoGebraParser
    {
        private String filename;

        public GeoGebraParser(String filename)
        {
            this.filename = filename;
        }

        public void Parse()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);

            XmlNode root = xmlDocument.DocumentElement;

            // Get intersection info
            Dictionary<string, IntersectionInfo> intersections = new Dictionary<string, IntersectionInfo>();
            string xpath = "//element[@type='point']";
            foreach(XmlNode intersectionXmlNode in root.SelectNodes(xpath))
            {
                IntersectionInfo intersectionInfo = new IntersectionInfo();
                if (intersectionXmlNode is XmlElement intersection)
                {
                    intersectionInfo.id = intersection.GetAttribute("label");
                    XmlNode coordsXmlNode = intersectionXmlNode.SelectSingleNode("coords");
                    if (coordsXmlNode is XmlElement coords)
                    {
                        
                        intersectionInfo.x = Int32.Parse(coords.GetAttribute("x"));
                        intersectionInfo.y = Int32.Parse(coords.GetAttribute("y"));
                    }
                }

                if (!intersections.ContainsKey(intersectionInfo.id))
                    intersections.Add(intersectionInfo.id, intersectionInfo);
            }

            foreach (IntersectionInfo i in intersections.Values)
            {
                Debug.WriteLine("Intersection {0}: ({1}, {2})", i.id, i.x, i.y);
            }
        }
    }
}
