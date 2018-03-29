<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="System.Web.UI.DataVisualization.Charting" %>
<%@ Import Namespace="System.Drawing"  %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Promotions</h2>

    <%
      Color[] colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Black, Color.Magenta, Color.Cyan };

      System.Web.UI.DataVisualization.Charting.Chart chart = new System.Web.UI.DataVisualization.Charting.Chart();
      chart.Width = 800;
      chart.Height = 500;
      chart.RenderType = RenderType.ImageTag;
      chart.Palette = ChartColorPalette.BrightPastel;

      chart.Titles.Add(new Title("Promotions of selected ESAR members"));
      chart.ChartAreas.Add("area");
      chart.ChartAreas[0].AxisY.Title = "Mission Hours";

      var offset = 0;
      List<Dictionary<DateTime, int>> data;
      List<string> names = (List<string>)ViewData["titles"];
      
      for (int i=0; i<names.Count; i++)
      {
        chart.Series.Add(new Series(names[i]) { ChartType = SeriesChartType.Line, Color = colors[i % colors.Length] });
      }

      data = (List<Dictionary<DateTime, int>>)ViewData["data"];
      for (int i = 0; i < data.Count; i++)
      {
        foreach (var pair in data[i])
        {
          chart.Series[i].Points.AddXY(pair.Key, pair.Value);          
        }
      }
      
      offset = data.Count;
      for (int i=0; i < offset; i++)
      {
        chart.Series.Add(new Series(names[i]+" promote") {
          ChartType = SeriesChartType.Point,
          MarkerSize = 8,
          MarkerStyle = MarkerStyle.Triangle,
          IsValueShownAsLabel = true,
          IsVisibleInLegend = false,
          Color = colors[i % colors.Length]
        });        
      }
      var p = (List<Dictionary<DateTimeOffset, Tuple<int,string>>>)ViewData["promotes"];
      for (int i = 0; i < p.Count; i++)
      {
        foreach (var pair in p[i])
        {
          if (!pair.Value.Item2.StartsWith("ESAR ") || !pair.Value.Item2.EndsWith("L")) continue;
          
          //int j = chart.Series[offset + 1].Points.Count;
          DataPoint pt = new DataPoint() { Label = pair.Value.Item2.Substring(5) };
          pt.SetValueXY(pair.Key.LocalDateTime, pair.Value.Item1);
          chart.Series[offset + i].Points.Add(pt);
          //chart.Series[offset + i].Points.AddXY(pair.Key, pair.Value.Item1);
         // chart.Series[offset + i].Points[j].Label = pair.Value.Item2;
        }
      }
      
      
      chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

      chart.Legends.Add("l1");
      
      chart.Page = this;
      HtmlTextWriter writer = new HtmlTextWriter(Page.Response.Output);
      chart.RenderControl(writer);
      
       %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
