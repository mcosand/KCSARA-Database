<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="MissionTypes" style="clear:both;">
  <asp:Chart ID="typeHistory" runat="server" style="float:left; padding-right:3em;" OnLoad="TypeBreakout_binding" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="1000px" Height="250px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Missions By Type" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss" />
    <Legends>
      <asp:Legend BackColor="Transparent" Alignment="Near" Docking="Bottom" Font="Trebuchet MS, 8.25pt, style=Bold"
        IsTextAutoFit="False" Name="Default" LegendStyle="Row">
      </asp:Legend>
    </Legends>
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
                     BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0">
        <AxisY LineColor="255,64,64,64">
          <MajorGrid LineColor="64, 64, 64, 64" />
          <MajorTickMark Enabled="False" />
        </AxisY>
        <AxisX Interval="1" IntervalType="Number" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="90" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart></div>
  <script runat="server">
    protected void TypeBreakout_binding(object sender, EventArgs e)
    {
        var data = (Dictionary<string,Dictionary<int,int>>)ViewData["typeHistory"];

        foreach (var series in data.OrderBy(f => f.Key))
        {

            Series s = new Series(series.Key)
            { ChartType = SeriesChartType.Spline, MarkerSize = 7, MarkerStyle = MarkerStyle.Circle };
            
            
            s.Points.DataBind(series.Value, "Key", "Value", null);
            typeHistory.Series.Add(s);
            typeHistory.DataManipulator.InsertEmptyPoints(1, IntervalType.Number, s);

            s.EmptyPointStyle.BorderWidth = 1;
            s.EmptyPointStyle.MarkerColor = System.Drawing.Color.FromArgb(0, 192, 0);
            s.EmptyPointStyle.CustomProperties = "EmptyPointValue = Zero";

        }
    }
  </script>

  <div>
      <asp:Chart ID="volCount" runat="server"  style="float:left; padding-right:3em;" OnLoad="VolCount_Loading" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="1000px" Height="250px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Number of Volunteers" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss" />
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
                     BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0" >
        <AxisY LineColor="255,64,64,64">
          <MajorGrid LineColor="64, 64, 64, 64" />
          <MajorTickMark Enabled="False" />
        </AxisY>
        <AxisX Interval="1" IntervalType="Number" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="90" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart></div>
    <script runat="server">
        protected void VolCount_Loading(object sender, EventArgs e)
    {
        var data = (Dictionary<int,int>)ViewData["volCount"];
        Series s = new Series("Volunteers") { ChartType = SeriesChartType.Spline, MarkerSize = 7, MarkerStyle = MarkerStyle.Circle, BorderWidth = 2 };
        Series avg = new Series("Average") { ChartType = SeriesChartType.Spline, BorderDashStyle = ChartDashStyle.Dash, Color = System.Drawing.Color.Gray };

        s.Points.DataBind(data.OrderBy(f => f.Key).ToArray(), "Key", "Value", null);
        volCount.Series.Add(s);
        volCount.Series.Add(avg);
        volCount.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "5", s, avg);            
    }
    </script>

  <div>
      <asp:Chart ID="volHours" runat="server"  style="float:left; padding-right:3em;" OnLoad="VolHours_Loading" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="1000px" Height="250px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Volunteer Hours" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss" />
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
                     BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0" >
        <AxisY LineColor="255,64,64,64">
          <MajorGrid LineColor="64, 64, 64, 64" />
          <MajorTickMark Enabled="False" />
        </AxisY>
        <AxisX Interval="1" IntervalType="Number" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="90" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart></div>
    <script runat="server">
        protected void VolHours_Loading(object sender, EventArgs e)
    {
        var data = (Dictionary<int,double?>)ViewData["volHours"];
        Series s = new Series("Hours") { ChartType = SeriesChartType.Spline, MarkerSize = 7, MarkerStyle = MarkerStyle.Circle, BorderWidth = 2 };
        Series avg = new Series("Average") { ChartType = SeriesChartType.Spline, BorderDashStyle = ChartDashStyle.Dash, Color = System.Drawing.Color.Gray };

        s.Points.DataBind(data.OrderBy(f => f.Key).ToArray(), "Key", "Value", null);
        volHours.Series.Add(s);
        volHours.Series.Add(avg);
        volHours.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "5", s, avg);            
    }
    </script>

</asp:Content>
