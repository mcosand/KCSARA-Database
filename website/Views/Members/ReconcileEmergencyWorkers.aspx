<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Reconcile Emergency Workers</h2>

    <p>Upload XLSX file containing current Emergency Workers. The file is expect to have the following format:</p>
    <table border="1" style="margin:1em;">
    <tr><th>EMT?</th><th>DEM #</th><th>LastName</th><th>FirstName</th><th>Common Name</th><th>ns1:OrgRank</th><th>ns1:Status</th></tr>
    </table>
    <% Html.BeginForm<MembersController>(f => f.ReconcileEmergencyWorkers(), FormMethod.Post, new { enctype = "multipart/form-data" }); %>
    <% if (ViewData["lastFileDate"] != null) {%>
    <h5>Press Upload without selecting a file to use the most recent data, uploaded <%: ViewData["lastFileDate"] %>.</h5>
    <% } %>
    <input type="file" name="file" /><br />
    <input type="checkbox" name="saveFile" value="true" />Save this file for future reference.<br />
    <br />
    <input type="submit" value="Upload" />
    <% Html.EndForm(); %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
