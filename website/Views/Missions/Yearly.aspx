<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
<style>
.r { text-align:right; }</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

  <script runat="server">
    RosterSummaryRow totals;
    IEnumerable<CountSummaryRow> missionTypes;
    IEnumerable<RosterSummaryRow> unitSummary;

    protected void Page_Load(object sender, EventArgs e)
    {
      totals = (RosterSummaryRow)ViewData["TotalSummary"];
      missionTypes = (IEnumerable<CountSummaryRow>)ViewData["TypeSummary"];
      unitSummary = (IEnumerable<RosterSummaryRow>)ViewData["UnitsSummary"];
    }
  
  </script>
<div id="TotalSummary">
<h2>Mission Totals:</h2>
  <table>
    <tr><th>Missions:</th><td><%= totals.Missions %></td></tr>
    <tr><th>Persons:</th><td><%= totals.Persons %></td></tr>
    <tr><th>Hours:</th><td><%= totals.Hours.ToString("0.00") %></td></tr>
    <tr><th>Miles:</th><td><%= totals.Miles %></td></tr>
  </table>
  </div>
  
  <div id="MissionTypes" style="clear:both;">
  <asp:Chart ID="TypeBreakout" runat="server" style="float:left; padding-right:3em;" OnLoad="TypeBreakout_binding" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="350px" Height="250px" BorderDashStyle="Solid" BackGradientStyle="TopBottom" 
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Missions By Type" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss" />
    <Legends>
      <asp:Legend BackColor="Transparent" Alignment="Near" Docking="Right" Font="Trebuchet MS, 8.25pt, style=Bold"
        IsTextAutoFit="False" Name="Default" LegendStyle="Column">
      </asp:Legend>
    </Legends>
    <Series>
      <asp:Series Name="s" ChartType="Pie" IsValueShownAsLabel="true">
      </asp:Series>
    </Series>
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
                     BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0">
        <AxisY LineColor="255,64,64,64">
          <MajorGrid LineColor="64, 64, 64, 64" />
          <MajorTickMark Enabled="False" />
          <LabelStyle Enabled="False" />
        </AxisY>
        <AxisX Interval="1" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false" IsReversed="true">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="90" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart>
  <script runat="server">
    protected void TypeBreakout_binding(object sender, EventArgs e)
    {
      TypeBreakout.Series["s"].Points.DataBind(this.missionTypes, "Key", "Count", null);
    }
  </script>
  
    <table>
      <tr><th>Type</th><th>Count</th></tr>
      <% foreach (CountSummaryRow row in this.missionTypes.OrderByDescending(f => f.Count))
           {       
      %>
      <tr><td><%= row.Key %></td><td><%= row.Count %></td></tr>
      <% }%>
    </table>
    <div style="clear:both; font-size:80%">&quot;Missions by type&quot; may sum to more than the
        number of missions, as some missions are of multiple types.</div>
  </div>
  
  <div id="UnitBreakout" style="clear:both;">
  <h2>Mission Response By Unit:</h2>
  <table id="unitRaw">
  <tr><th>Unit</th><th>Missions</th><th>Responders</th><th>Hours</th><th>Miles</th></tr>
  <% foreach (RosterSummaryRow row in unitSummary)
     { %>
  <tr>
    <td><%= row.Title %></td>
    <td class="r"><%= row.Missions%></td>
    <td class="r"><%= row.Persons%></td>
    <td class="r"><%= row.Hours.ToString("0.00") %></td>
    <td class="r"><%= row.Miles%></td>
  </tr>
  <% } %>
  </table>
  
  <asp:Chart ID="unitHoursChart" runat="server" OnLoad="UnitsHours_binding" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="550px" Height="400px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Hours by Unit" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss"></BorderSkin>
    <Series>
      <asp:Series Name="s" IsValueShownAsLabel="true" ChartType="Bar"
        BorderColor="180, 26, 59, 105" Color="220, 65, 140, 240" LabelFormat="{0:0}">
      </asp:Series>
    </Series>
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
        BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0">
        <%--	<area3dstyle Rotation="0" Enable3D="true" /> --%>
        <AxisY Enabled="False" />
        <AxisX Interval="1" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false" IsReversed="true">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="45" IsStaggered="true" />
          <MajorGrid LineColor="64, 64, 64, 64" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart>

  <asp:Chart ID="unitRespondersChart" runat="server" OnLoad="UnitsResponders_binding" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="550px" Height="400px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Responders by Unit" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss"></BorderSkin>
    <Series>
      <asp:Series Name="s" IsValueShownAsLabel="true" ChartType="Bar"
        BorderColor="180, 26, 59, 105" Color="220, 65, 140, 240">
      </asp:Series>
    </Series>
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
        BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0">
        <%--	<area3dstyle Rotation="0" Enable3D="true" /> --%>
        <AxisY Enabled="False" />
        <AxisX Interval="1" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false" IsReversed="true">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="45" IsStaggered="true" />
          <MajorGrid LineColor="64, 64, 64, 64" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart>

  <asp:Chart ID="unitMissionsChart" runat="server" OnLoad="UnitsMissions_binding" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="550px" Height="280px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Missions by Unit" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss"></BorderSkin>
    <Series>
      <asp:Series Name="s" IsValueShownAsLabel="true" ChartType="Bar"
        BorderColor="180, 26, 59, 105" Color="220, 65, 140, 240">
      </asp:Series>
    </Series>
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
        BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0">
        <%--	<area3dstyle Rotation="0" Enable3D="true" /> --%>
        <AxisY Enabled="False" />
        <AxisX Interval="1" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false" IsReversed="true">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="45" IsStaggered="true" />
          <MajorGrid LineColor="64, 64, 64, 64" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart>
  
    <asp:Chart ID="unitMilesChart" runat="server" OnLoad="UnitsMiles_binding" Palette="BrightPastel"
    BackColor="#F3DFC1" Width="550px" Height="280px" BorderDashStyle="Solid" BackGradientStyle="TopBottom"
    BorderWidth="2" BorderColor="181, 64, 1">
    <Titles>
      <asp:Title ShadowColor="32, 0, 0, 0" Font="Trebuchet MS, 14.25pt, style=Bold" ShadowOffset="3"
        Text="Miles by Unit" ForeColor="26, 59, 105">
      </asp:Title>
    </Titles>
    <BorderSkin SkinStyle="Emboss"></BorderSkin>
    <Series>
      <asp:Series Name="s" IsValueShownAsLabel="true" ChartType="Bar"
        BorderColor="180, 26, 59, 105" Color="220, 65, 140, 240">
      </asp:Series>
    </Series>
    <ChartAreas>
      <asp:ChartArea Name="a" BorderColor="64, 64, 64, 64" BackSecondaryColor="Transparent"
        BackColor="Transparent" ShadowColor="Transparent" BorderWidth="0">
        <%--	<area3dstyle Rotation="0" Enable3D="true" /> --%>
        <AxisY Enabled="False" />
        <AxisX Interval="1" LineColor="64, 64, 64, 64" TextOrientation="Stacked" IsMarginVisible="false" IsReversed="true">
          <LabelStyle Font="Trebuchet MS, 13pt, style=Bold" Angle="45" IsStaggered="true" />
          <MajorGrid LineColor="64, 64, 64, 64" />
        </AxisX>
      </asp:ChartArea>
    </ChartAreas>
  </asp:Chart>

  <script runat="server">
    protected void UnitsHours_binding(object sender, EventArgs e)
    {
      unitHoursChart.Series["s"].Points.DataBind(unitSummary, "Title", "Hours", null);
    }
    protected void UnitsResponders_binding(object sender, EventArgs e)
    {
      unitRespondersChart.Series["s"].Points.DataBind(unitSummary, "Title", "Persons", null);
    }
    protected void UnitsMissions_binding(object sender, EventArgs e)
    {
      unitMissionsChart.Series["s"].Points.DataBind(unitSummary, "Title", "Missions", null);
    }
    protected void UnitsMiles_binding(object sender, EventArgs e)
    {
      unitMilesChart.Series["s"].Points.DataBind(unitSummary, "Title", "Miles", null);
    }
  </script>
  </div>
</asp:Content>
