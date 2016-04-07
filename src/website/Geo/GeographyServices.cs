/*
 * Copyright 2010-2014 Matthew Cosand
 */
namespace Kcsara.Database.Geo
{
  using System;
  using System.Configuration;
  using System.Data.SqlTypes;
  using System.Linq;
  using System.Net;
  using System.Text.RegularExpressions;
  using System.Web;
  using System.Xml;
  using System.Xml.Linq;
  using Kcsar.Database.Model;
  using log4net;
  using Microsoft.SqlServer.Types;

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
      MapsLookupResult maps = GeographyServices.GeocodeAddress(addr);
      if (maps.Result == LookupResult.Success)
      {
        addr.Quality = maps.Quality;
        addr.Location = maps.Geography;
      }
    }

    public static MapsLookupResult GeocodeAddress(IAddress addr)
    {
      MapsLookupResult result = new MapsLookupResult { Result = LookupResult.Error };
      string url = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?address={0}&key={1}",
        System.Web.HttpUtility.UrlEncode(string.Format("{0}, {1} {2} {3}", addr.Street, addr.City, addr.State, addr.Zip)),
        System.Configuration.ConfigurationManager.AppSettings["Maps3Key"]
      );

      try
      {
        if (addr.Street.ToLower().StartsWith("po box"))
        {
          result.Result = LookupResult.NotFound;
          return result;
        }

        WebClient client = new WebClient();
        XmlDocument xml = new System.Xml.XmlDocument();

        xml.LoadXml(client.DownloadString(url));

        string status = xml.SelectSingleNode("/GeocodeResponse/status").InnerText;
        if (status != "OK")
        {
          throw new InvalidOperationException(xml.OuterXml);
        }
        else
        {
          var results = xml.SelectNodes("/GeocodeResponse/result");
          if (results.Count == 0)
          {
            LogManager.GetLogger("GeographyServices").InfoFormat("No results for {0}", url);
            result.Result = LookupResult.NotFound;
          }
          else if (results.Count > 1)
          {
            result.Result = LookupResult.Range;
            LogManager.GetLogger("GeographyServices").InfoFormat("Multiple results for {0}", url);
          }
          else
          {
            result.Result = LookupResult.Success;
            var coordType = results[0].SelectSingleNode("geometry/location_type").InnerText;
            if (coordType == "ROOFTOP")
            {
              result.Quality = (int)GeocodeQuality.High;
            }
            else if (coordType == "RANGE_INTERPOLATED")
            {
              result.Quality = (int)GeocodeQuality.Medium;
            }
            else 
            {
              result.Quality = (int)GeocodeQuality.Poor;
              LogManager.GetLogger("GeographyServices").WarnFormat("Poor quality geocode: {0}", url);
            }

            if (result.Quality > (int)GeocodeQuality.Poor)
            {
              LogManager.GetLogger("GeographyServices").InfoFormat("Got location: {0},{1}", results[0].SelectSingleNode("geometry/location/lat").InnerText, results[0].SelectSingleNode("geometry/location/lat").InnerText);
              double lat = double.Parse(results[0].SelectSingleNode("geometry/location/lat").InnerText);
              double lng = double.Parse(results[0].SelectSingleNode("geometry/location/lng").InnerText);

              // This call is slow. Might be good to move to background task.
              // double z = GeographyServices.GetElevation(lat, lng);
              double z = -100;


              string point = (z > 0) ? string.Format("POINT({0} {1} {2} NULL)", lng, lat, z) : string.Format("POINT({0} {1})", lng, lat);

              result.Geography = SqlGeography.STGeomFromText(new SqlChars(point.ToCharArray()), GeographyServices.SRID);
            }
          }
        }
      }
      catch (Exception)
      {
        LogManager.GetLogger("GeographyServices").InfoFormat("Error geocoding address: {0}", url);
        throw;
      }
      return result;
    }

    public static double GetElevation(double lat, double lng)
    {
      WebClient client = new WebClient();
      string url = string.Format("http://http://ned.usgs.gov/epqs/pqs.php?x={0}&y={1}&units=Meters&output=xml",
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

      Match m = Regex.Match(result, "<Elevation>(.*)</Elevation>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

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
        html = client.DownloadString(string.Format("https://tools.usps.com/go/ZipLookupResultsAction!input.action?resultMode=0&companyName=&address1={0}&address2=&city={1}&state={2}&urbanCode=&postalCode=&zip={3}",
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
      /*
       * <div class="data"> 

            <p class="std-address">
										
										
										
                <span class="address1 range">STREET ADDRESS RESULT</span><br />
										
										
              <span class="city range">CITY RESULT</span> <span class="state range">WA</span> <span class="zip" style="">ZIP5 RESULT</span><span class="hyphen">&#45;</span><span class="zip4">ZIP+4 RESULT</span>
*/
      var match = Regex.Match(Regex.Replace(html, "[\\r\\n\\s]+", " "), "<div class=\"data\">(.*?)</div> </li>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      if (match.Success == false) throw new InvalidOperationException("Unexpected result format: \n" + html);

      XDocument matchBody = XDocument.Parse(match.Groups[1].Value.Replace("&trade;", ""));

      result.Street = matchBody.Descendants().First(f => f.Attribute("class") != null && f.Attribute("class").Value.Contains("address1")).Value;
      result.City = matchBody.Descendants().First(f => f.Attribute("class") != null && f.Attribute("class").Value.Contains("city")).Value;
      result.State = matchBody.Descendants().First(f => f.Attribute("class") != null && f.Attribute("class").Value.Contains("state")).Value;
      result.Zip = matchBody.Descendants().First(f => f.Attribute("class") != null && f.Attribute("class").Value.Contains("zip")).Value;
      result.Result = LookupResult.Success;

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
