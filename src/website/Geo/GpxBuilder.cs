/*
 * Copyright 2011-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace Kcsara.Database.Geo
{
    public class GpxDocument
    {
        private XmlDocument doc;
        private XmlElement current;
        private GpxState state = GpxState.Root;

        public GpxDocument()
        {
            doc = new XmlDocument();
            doc.LoadXml(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no"" ?>
<gpx xmlns=""http://www.topografix.com/GPX/1/1"" creator=""KCSARA Database"" version=""1.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd""></gpx>");
            doc.PreserveWhitespace = true;
            current = doc.DocumentElement;
        }

        public void AppendToTrack(double latitude, double longitude)
        {
            AppendToTrack(latitude, longitude, null, null);
        }

        public void AppendToTrack(double latitude, double longitude, double? altitude, DateTime? time)
        {
            if (state != GpxState.Track)
            {
                throw new InvalidOperationException();
            }

            XmlElement pt = doc.CreateElement("trkpt");
            pt.SetAttribute("lat", latitude.ToString());
            pt.SetAttribute("lon", longitude.ToString());
            if (altitude.HasValue)
            {
                XmlElement a = doc.CreateElement("time");
                a.InnerText = altitude.Value.ToString();
                pt.AppendChild(a);
            }
            if (time.HasValue)
            {
                XmlElement t = doc.CreateElement("time");
                t.InnerText = time.Value.ToUniversalTime().ToString(@"yyyy-MM-ddTHH:mm:ssZ");
                pt.AppendChild(t);
            }
            current.AppendChild(pt);
        }

        public void StartTrack(string name)
        {
            if (state != GpxState.Root)
            {
                throw new InvalidOperationException();
            }
            XmlElement trkNode = doc.CreateElement("trk");
            XmlElement nameNode = doc.CreateElement("name");
            nameNode.InnerText = name;
            trkNode.AppendChild(nameNode);

            XmlElement segNode = doc.CreateElement("trkseg");
            trkNode.AppendChild(segNode);
            current.AppendChild(trkNode);
            current = segNode;
            state = GpxState.Track;
        }

        public void FinishTrack()
        {
            if (state != GpxState.Track)
            {
                throw new InvalidOperationException();
            }
            current = doc.DocumentElement;
            state = GpxState.Root;
        }

        public byte[] ToUtf8()
        {
            return System.Text.Encoding.UTF8.GetBytes(doc.OuterXml.Replace(" xmlns=\"\"", ""));
        }

        private enum GpxState
        {
            Root,
            Track
        }
    }
}
