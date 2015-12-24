<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Kcsara.Database.Web.Model.MeshNodeStatus[]>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>MeshStatus</h2>
    <% foreach (var node in Model.OrderBy(f => f.Name)) { %>
    <div>
    <h3><%= node.Name %></h3>
    <table cellpadding="0"><tr>
    <td>
    <b>Last Seen:</b> <%= node.Time.ToString("HH:mm MMM d") %><br/>
    <b>IP Address:</b> <%= node.IPAddr.Replace("\n", "<br/>") %><br />
    <b>Uptime:</b> <%= TimeSpan.FromSeconds((long)node.Uptime).ToString() %><br />
<% if (node.Location != null) { %>
    <a target="_blank" href="http://www.bing.com/maps/?v=2&lvl=17&sty=h&where1=<%= node.Location.Latitude %>, <%= node.Location.Longitude %>&cp=<%= node.Location.Latitude %>~<%= node.Location.Longitude %>">
    <img style="border:solid 1px black;" src="https://dev.virtualearth.net/REST/V1/Imagery/Map/Road/<%= node.Location.Latitude %>,<%= node.Location.Longitude %>/14?mapSize=200,200&key=AscK_yoaqpFk3Up2F0pDDnTNdpMXvmAfOVE-JqeZbC-H_RzGfyajPAB_Z3IHw4lH" /></a><br/>
<% } %>
    <%= Html.ActionLink("Recent Track", "MeshNodeTrack", new { id = node.Name }) %></td>
    <td><a href="<%= Url.Action("MeshGraphs", new { id=node.Name, type="volts" }) %>"><img src="<%= Url.Content("~/Content/auth/nodes/"+node.Name+"-voltsdaily.gif") %>" /></a><br />
    <a href="<%= Url.Action("MeshGraphs", new { id=node.Name }) %>"><img src="<%= Url.Content("~/Content/auth/nodes/"+node.Name+"-daily.gif") %>" /></a><br />
    </td>
    </tr></table>
    </div>
    <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
