/*
 * Copyright 2010-2014 Matthew Cosand
 */
namespace Kcsara.Database.Geo
{
    using System;
    using System.Data.SqlTypes;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using Kcsar.Database.Model;
    using Microsoft.SqlServer.Types;
    using System.Configuration;
    using Kcsar.Membership;

    public static class GeographyServices
    {
        public const int SRID = 4326;

        public static void CopyFrom(this IAddress to, IAddress from)
        {
            to.Street = from.Street;
            to.City = from.City;
            to.State = from.State;
            to.Zip = from.Zip;
        }

        public static string FormatCoordinate(double coord, CoordinateDisplay display)
        {
            coord = Math.Abs(coord);
            if (display == CoordinateDisplay.DecimalDegrees)
            {
                return string.Format("{0:0.00000}", coord);
            }
            else if (display == CoordinateDisplay.DecimalMinutes)
            {
                return string.Format("{0} {1:0.000}", (int)coord, (coord - (int)coord) * 60.0);
            }
            return "Unknown format";
        }

        public static void RefineAddressWithGeography(IAddressGeography addr)
        {
            UspsLookupResult usps = GeographyServices.LookupUspsAddress(addr);
            if (usps.Result == LookupResult.Success)
            {
                addr.Quality = (addr.Quality & 0xfff0) | 0x01;
                addr.CopyFrom(usps);

                Match suffixMatch = Regex.Match(addr.Street, "^(.+) (APT|BSMT|#|BLDG|DEPT|FL|FRNT|HNGR|KEY|LBBY|LOT|LOWR|OFC|PH|PIER|REAR|RM|SIDE|SLIP|SPC|STOP|STE|TRLR|UNIT|UPPR)(.*)$");
                string suffix = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(suffixMatch.Groups[2].Value.ToLowerInvariant())
                                      + suffixMatch.Groups[3].Value;

                MapsLookupResult maps = GeographyServices.GeocodeAddress(usps);
                if (maps.Result == LookupResult.Success)
                {
                    addr.Quality = (addr.Quality & 0xff0f) | (maps.Quality << 4);
                    string mapsStreet = maps.Street + (suffixMatch.Success ? (" " + suffix) : "");
                    if (addr.Street.Equals(mapsStreet, StringComparison.OrdinalIgnoreCase))
                    {
                        addr.CopyFrom(maps);
                        addr.Street = mapsStreet;
                    }
                    addr.Location = maps.Geography;
                }
            }
        }

        public static MapsLookupResult GeocodeAddress(IAddress addr)
        {
            MapsLookupResult result = new MapsLookupResult { Result = LookupResult.Error };
            if (addr.Street.ToLower().StartsWith("po box"))
            {
                result.Result = LookupResult.NotFound;
                return result;
            }

            WebClient client = new WebClient();
            XmlDocument xml = new System.Xml.XmlDocument();
            string url = string.Format("http://maps.google.com/maps/geo?q={0}&key={1}&output=xml",
                System.Web.HttpUtility.UrlEncode(string.Format("{0}, {1} {2} {3}", addr.Street, addr.City, addr.State, addr.Zip)),
                System.Configuration.ConfigurationManager.AppSettings["MapsKey"]
                );

            xml.LoadXml(client.DownloadString(url));

            XmlElement root = xml.DocumentElement;
            XmlNamespaceManager nsmgr = new
            XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("k", root.NamespaceURI);
            nsmgr.AddNamespace("d", "urn:oasis:names:tc:ciq:xsdschema:xAL:2.0");


            string status = xml.SelectSingleNode("//k:Response/k:Status/k:code", nsmgr).InnerText;
            if (status != "200")
            {
                // Error
            }
            else
            {
                XmlNode details = xml.SelectSingleNode("//d:AddressDetails", nsmgr);

                string accuracyString = ((XmlElement)details).GetAttribute("Accuracy");
                result.Quality = int.Parse(accuracyString);

                result.Result = LookupResult.Range;
                if (result.Quality > 5)
                {
                    result.Result = LookupResult.Success;
                    result.Street = details.SelectSingleNode("//d:ThoroughfareName", nsmgr).InnerText;
                }
                if (result.Quality > 4)
                {
                    result.Zip = details.SelectSingleNode("//d:PostalCodeNumber", nsmgr).InnerText;
                }
                result.City = details.SelectSingleNode("//d:LocalityName", nsmgr).InnerText;
                result.State = details.SelectSingleNode("//d:AdministrativeAreaName", nsmgr).InnerText;

                string[] coords = details.SelectSingleNode("//k:Response/k:Placemark/k:Point", nsmgr).InnerText.Split(',');
                double lat = double.Parse(coords[1]);
                double lng = double.Parse(coords[0]);
                double z = GeographyServices.GetElevation(lat, lng);

                string point = (z > 0) ? string.Format("POINT({0} {1} {2} NULL)", lng, lat, z) : string.Format("POINT({0} {1})", lng, lat);

                result.Geography = SqlGeography.STGeomFromText(new SqlChars(point.ToCharArray()), GeographyServices.SRID);
            }
            return result;
        }

        public static double GetElevation(double lat, double lng)
        {
            WebClient client = new WebClient();
            string url = string.Format("http://gisdata.usgs.gov/xmlwebservices2/elevation_service.asmx/getElevation?X_Value={0}&Y_Value={1}&Elevation_Units=meters&Elevation_Only=true&Source_Layer=-1",
                lng, lat);
            string result = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    result = client.DownloadString(url);
                    break;
                }
                catch (WebException)
                {
                }
            }
            if (result == null)
            {
                return -100;
            }

            Match m = Regex.Match(result, "<double>(.*)</double>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return double.Parse(m.Groups[1].Value);
        }

        public static UspsLookupResult LookupUspsAddress(IAddress addr)
        {
            UspsLookupResult result = new UspsLookupResult { Result = LookupResult.Error };
            System.Net.WebClient client = new System.Net.WebClient();

            int retries = 3;
            string html = "";
            while (retries-- > 0)
            {
                html = client.DownloadString(string.Format("http://zip4.usps.com/zip4/zcl_0_results.jsp?visited=1&pagenumber=0&firmname=&address2={0}&address1=&city={1}&state={2}&urbanization=&zip5={3}",
                    HttpUtility.UrlEncode(addr.Street),
                    HttpUtility.UrlEncode(addr.City),
                    HttpUtility.UrlEncode(addr.State),
                    HttpUtility.UrlEncode(addr.Zip)));
                if (!html.ToLowerInvariant().Contains("error page"))
                {
                    break;
                }
                html = "";
            }

            MatchCollection matches = Regex.Matches(html.Replace("\n", ""), "<tr>\\s*<td[^>]+headers=\"(.*?)\"(.*?)</tr>", RegexOptions.IgnoreCase);

            if (matches.Count == 1)
            {
                if (matches[0].Groups[1].Value.ToLowerInvariant() == "full")
                {
                    Match sub = Regex.Match(matches[0].Groups[2].Value,
                        ">(.*?)</td>", RegexOptions.IgnoreCase);
                    if (sub.Success)
                    {
                        string[] fields = Regex.Split(sub.Groups[1].Value, @"<\s*br\s*\/?\s*>", RegexOptions.IgnoreCase);
                        result.Street = fields[0].Trim();
                        string[] others = fields[1].Split(new[] { "&nbsp;" }, StringSplitOptions.None);

                        result.City = others[0].Trim();
                        result.State = others[1].Trim();
                        result.Zip = others[3].Trim();

                        if (!string.IsNullOrEmpty(others[2]))
                        {
                            throw new InvalidOperationException("Found 3rd field: " + others[2].Trim());
                        }

                        result.Result = LookupResult.Success;
                    }
                    else
                    {
                        result.Result = LookupResult.Error;
                    }
                }
                else
                {
                    result.Result = LookupResult.Error;
                }
            }
            else if (matches.Count > 1)
            {
                result.Result = LookupResult.Range;
            }

            return result;
        }

        public static SqlGeography GetDefaultLocation()
        {
            string centerLat = ConfigurationManager.AppSettings["mapCenterLat"] ?? "47.592557";
            string centerLong = ConfigurationManager.AppSettings["mapCenterLong"] ?? "-121.837041";

            return SqlGeography.Point(double.Parse(centerLat), double.Parse(centerLong), SRID);
        }
    }
}
