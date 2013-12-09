<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Dictionary<Member,CompositeTrainingStatus>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Required Training Report</h2>
  <div class="perf"><%= ViewData["perf"] %>s</div>

  <%
    IDictionary<SarUnit, IList<Member>> unitTable = (IDictionary<SarUnit, IList<Member>>)ViewData["unitTable"];
    foreach (SarUnit u in unitTable.Keys.OrderBy(x => x.DisplayName)) 
    { %>
  <h3><%= u.DisplayName%></h3>
  <table>
  <thead>
    <tr><th>DEM</th><th colspan="2">Name</th><th>WAC Type</th><th><%= string.Join("</th><th>", ((string[])ViewData["courseList"]))%></th></tr>
  </thead>
  <tbody>
  <% foreach (Member m in unitTable[u].OrderBy(x => x.ReverseName))
     {
       bool bad = !ViewData.Model[m].IsGood;
        %>
    <tr>
      <td><%= m.DEM %></td>
      <td style="white-space:nowrap;"><%= Html.ActionLink<MembersController>(x => x.Detail(m.Id), m.ReverseName)%></td>
      <td style="color:Red; font-weight:bold;"><%= bad ? "X" : ""%></td>
      <td class="wac_<%= m.WacLevel %>"><%= m.WacLevel%></td>
      <% foreach (string course in (string[])ViewData["courseList"])
      {
        TrainingStatus stat = Model[m].Expirations.Values.Where(f => f.CourseName == course).Single();
        Response.Write(string.Format("<td class=\"exp_{0}\">{1}</td>", stat.Status, stat.ToString()));
      }
      %>
    </tr>
  <% } %>
  </tbody>
  </table>
  <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
