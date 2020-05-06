using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RoadTrafficSimulator.Simulator.WorldEntities;
using RoadTrafficSimulator.Simulator.DataStructures.LinAlg;

namespace RoadTrafficSimulator.WorldData
{
    public struct IntersectionInfo
    {
        public string id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }

    public struct RoadInfo
    {
        public string id { get; set; }
        public int numLanesIn { get; set; }
        public int numLanesOut { get; set; }
        public float speedLimit { get; set; }
    }

    class GeoGebraParser
    {
        private readonly XmlNode root;
        public GeoGebraParser(string filename)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);

            XmlNode documentRoot = xmlDocument.DocumentElement;
            this.root = documentRoot.SelectSingleNode("construction");
        }

        public Tuple<List<FourWayIntersection>, List<Road>> Parse()
        {
            // Get intersection info
            Dictionary<string, IntersectionInfo> intersectionInfo = ParseIntersections();
            // Get road info
            Dictionary<string, RoadInfo> roadInfo = ParseRoadInfo();
            // Get connections (roadId -> (sourceIntersectionId, targetIntersectionId)
            Dictionary<string, Tuple<string, string>> roadConn = ParseRoadConnections();

            Dictionary<string, FourWayIntersection> intersections = new Dictionary<string, FourWayIntersection>();
            foreach (IntersectionInfo i in intersectionInfo.Values)
            {
                Vector2 origin = new Vector2(i.x, i.y);
                FourWayIntersection intersection = new FourWayIntersection(origin);

                intersections.Add(i.id, intersection);
            }

            List<Road> roads = new List<Road>();
            foreach (RoadInfo r in roadInfo.Values)
            {
                FourWayIntersection source = intersections[roadConn[r.id].Item1];
                FourWayIntersection target = intersections[roadConn[r.id].Item2];
                Road road = new Road(ref source, ref target, r.numLanesIn, r.numLanesOut, r.speedLimit);
                roads.Add(road);
            }
            return new Tuple<List<FourWayIntersection>, List<Road>>(intersections.Values.ToList(), roads);
        }

        public Dictionary<string, IntersectionInfo> ParseIntersections()
        {
            Dictionary<string, IntersectionInfo> intersections = new Dictionary<string, IntersectionInfo>();
            string xpath = "//element[@type='point']";
            foreach (XmlNode intersectionXmlNode in root.SelectNodes(xpath))
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

            return intersections;
        }

        public Dictionary<string, RoadInfo> ParseRoadInfo()
        {
            Dictionary<string, RoadInfo> roads = new Dictionary<string, RoadInfo>();
            string xpath = "//element[@type='segment']";
            foreach (XmlNode roadXmlNode in root.SelectNodes(xpath))
            {
                RoadInfo roadInfo = new RoadInfo();
                if (roadXmlNode is XmlElement road)
                {
                    roadInfo.id = road.GetAttribute("label");
                    XmlNode captionXmlNode = roadXmlNode.SelectSingleNode("caption");
                    if (captionXmlNode is XmlElement caption)
                    {
                        string val = caption.GetAttribute("val");
                        string[] csv = val.Split(',');
                        roadInfo.numLanesIn = Int32.Parse(csv[0]);
                        roadInfo.numLanesOut = Int32.Parse(csv[1]);
                        roadInfo.speedLimit = float.Parse(csv[2]);
                    }
                }

                if (!roads.ContainsKey(roadInfo.id))
                    roads.Add(roadInfo.id, roadInfo);
            }

            return roads;
        }


        public Dictionary<string, Tuple<string, string>> ParseRoadConnections()
        {
            Dictionary<string, Tuple<string, string>> roadConnections = new Dictionary<string, Tuple<string, string>>();
            string xpath = "//command";
            foreach (XmlNode roadConnectionXmlNode in root.SelectNodes(xpath))
            {
                String roadId = "", sourceId = "", targetId = "";
                if (roadConnectionXmlNode.SelectSingleNode("output") is XmlElement roadIdElement)
                {
                    roadId = roadIdElement.GetAttribute("a0");
                }

                if (roadConnectionXmlNode.SelectSingleNode("input") is XmlElement intersectionsElement)
                {
                    sourceId = intersectionsElement.GetAttribute("a0");
                    targetId = intersectionsElement.GetAttribute("a1");
                }

                if (roadId.Length > 0 && sourceId.Length > 0 && targetId.Length > 0)
                    roadConnections.Add(roadId, new Tuple<string, string>(sourceId, targetId));
            }
            return roadConnections;
        }
    }
}
