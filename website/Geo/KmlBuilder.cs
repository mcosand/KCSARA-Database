using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Kcsara.Database.Geo
{
    using Kcsar.Database.Model;
    using Kcsara.Database.Web.Model;
    using Microsoft.SqlServer.Types;

    public class KmlBuilder
    {
        private readonly XNamespace KML_NS = "http://earth.google.com/kml/2.1";
        private readonly XNamespace GEXT_NS = "http://www.google.com/kml/ext/2.2";
        private readonly string[] colors = "ff4500,ff42c3,904cff".Split(',');

        private XDocument document;
        private XElement documentNode;
        private Dictionary<Guid, XElement> folders = new Dictionary<Guid, XElement>();
        private string[] knownIconTypes;
        string imagesUrl = "";

        public KmlBuilder()
        {
            document = new XDocument();

            var kmlNode = new XElement(KML_NS + "kml", new XAttribute(XNamespace.Xmlns + "gx", GEXT_NS.NamespaceName));

            document.Add(kmlNode);

            documentNode = Create("Document", "Unnamed File", "");
            kmlNode.Add(documentNode);
            this.knownIconTypes = new[] { "base", "found", "clue" };
            folders[Guid.Empty] = documentNode;
        }

        public KmlBuilder AddIconStyles(string imagesUrl)
        {
            this.imagesUrl = imagesUrl.TrimEnd('/');

            foreach (string type in knownIconTypes)
            {
                var iconNode = new XElement(KML_NS + "Style",
                    new XAttribute("id", type),
                    new XElement(KML_NS + "IconStyle",
                        new XElement(KML_NS + "Icon",
                            new XElement(KML_NS + "href", this.imagesUrl + "/" + type + "-e.png")
                        )
                    ),
                    new XElement(KML_NS + "LabelStyle",
                        new XElement(KML_NS + "scale", "0.6")
                    )
                );
                this.documentNode.Add(iconNode);
            }
            return this;
        }

        public string Name
        {
            get { return documentNode.Element(KML_NS + "name").Value; }
            set { documentNode.Element(KML_NS + "name").SetValue(value ?? ""); }
        }

        public string Description
        {
            get { return documentNode.Element(KML_NS + "description").Value; }
            set { documentNode.Element(KML_NS + "description").SetValue(value ?? ""); }
        }

        public string GetLineStyle(string color)
        {
            string styleName = "_" + color;

            if (documentNode.Elements(KML_NS + "Style").SingleOrDefault(f => f.Attribute("id").Value == styleName) == null)
            {
                var styleNode = new XElement(KML_NS + "Style");
                styleNode.SetAttributeValue("id", styleName);
                styleNode.Add(new XElement(KML_NS + "IconStyle",
                    new XElement(KML_NS + "Icon",
                        new XElement(KML_NS + "href", this.imagesUrl + "/track.png")
                    )
                ));
                styleNode.Add(new XElement(KML_NS + "LabelStyle",
                    new XElement(KML_NS + "scale", "0.6")
                ));
                var lineNode = new XElement(KML_NS + "LineStyle");
                styleNode.Add(lineNode);
                lineNode.Add(new XElement(KML_NS + "color", "88" + color));
                lineNode.Add(new XElement(KML_NS + "width", 3));
                documentNode.Add(styleNode);
            }
            return styleName;
        }

        public void AddItem(GeographyView item, string name, Guid? folder)
        {
            AddItem(item, name, item.Description, folder);
        }

        public int ColorIndex { get; set; }

        public void AddItem(GeographyView item, string name, string description, Guid? folder)
        {
            AddItem(item, name, null, description, folder);
        }

        public void AddItem(GeographyView item, string name, string kind, string description, Guid? folder)
        {
            bool visible = true;
            kind = (kind ?? "").ToLowerInvariant();
            TimeZoneInfo pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var itemNode = Create("Placemark", name, description);

            if (documentNode.Elements(KML_NS + "Style").SingleOrDefault(f => f.Attribute("id").Value == "noLabel") == null)
            {
                var styleNode = new XElement(KML_NS + "Style");
                styleNode.SetAttributeValue("id", "noLabel");
                styleNode.Add(new XElement("LabelStyle", new XElement("scale", "0.5")));
                documentNode.Add(styleNode);
            }

            if (item is RouteView)
            {
                List<string> coords = new List<string>();
                List<string> times = new List<string>();
                bool useTimestamp = true;

                itemNode.Add(new XElement(KML_NS + "styleUrl", "#" + GetLineStyle(colors[ColorIndex++ % colors.Length])));
                var lineNode = new XElement(GEXT_NS + "Track");
                itemNode.Add(lineNode);

                if (((RouteView)item).Points.All(f => f.Z.HasValue))
                {
                    // lineNode.Add(new XElement(KML_NS + "altitudeMode", "absolute"));
                }

                foreach (var pt in ((RouteView)item).Points)
                {
                    coords.Add(string.Format("{0} {1} {2}", pt.Long, pt.Lat, pt.Z));
                    if (useTimestamp && pt.Time.HasValue)
                    {
                        times.Add((pt.Time.Value - pacific.GetUtcOffset(pt.Time.Value)).ToString("s") + "Z");
                    }
                    else
                    {
                        useTimestamp = false;
                    }
                }

                if (useTimestamp)
                {
                    foreach (string time in times)
                    {
                        lineNode.Add(new XElement(KML_NS + "when", time));
                    }
                }

                foreach (string coord in coords)
                {
                    lineNode.Add(new XElement(GEXT_NS + "coord", coord));
                }
                //var lineNode = new XElement(KML_NS + "LineString");
                //lineNode.Add(new XElement(KML_NS + "extrude", 1));
                //lineNode.Add(new XElement(KML_NS + "tessellate", 1));
                //lineNode.Add(new XElement(KML_NS + "altitudeMode", "clampToGround"));
                //lineNode.Add(new XElement(KML_NS + "coordinates", string.Join("\n", ((RouteView)item).Points.Select(f => string.Format("{0},{1},10", f.Long, f.Lat)).ToArray())));
                itemNode.Add(lineNode);
            }
            else if (item is WaypointView)
            {
                WaypointView wpt = (WaypointView)item;
                itemNode.Add(new XElement(KML_NS + "Point", new XElement(KML_NS + "coordinates", string.Format("{0},{1}", wpt.Long, wpt.Lat))));
                if (item.Kind == "team")
                {
                    visible = false;
                }

                itemNode.Add(new XElement(KML_NS + "styleUrl", "#" + (knownIconTypes.Contains(kind) ? kind : "noLabel")));
            }

            itemNode.Add(new XElement(KML_NS + "visibility", visible ? 1 : 0));

            folders[folder ?? Guid.Empty].Add(itemNode);
        }

        public Guid CreateFolder(string name, Guid? parent)
        {
            XElement folder = Create("Folder", name, "");
            Guid key = Guid.NewGuid();

            folders.Add(key, folder);
            folders[parent ?? Guid.Empty].Add(folder);
            return key;
        }

        public void AddLink(string name, string description, string url, bool isVisible, Guid? folder)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL can not be empty", "url");
            }

            XElement link = Create("NetworkLink", name, description);
            if (!isVisible)
            {
                link.Add(new XElement(KML_NS + "visibility", 0));
            }

            link.Add(new XElement(KML_NS + "Url", new XElement(KML_NS + "href", url)));

            folders[folder ?? Guid.Empty].Add(link);
        }

        public override string ToString()
        {
            return document.ToString();
        }

        private XElement Create(string tag, string name, string description)
        {
            XElement node = new XElement(KML_NS + tag);

            node.Add(new XElement(KML_NS + "name", name));
            if (description != null)
            {
                node.Add(new XElement(KML_NS + "description", description));
            }

            return node;
        }
    }
}
