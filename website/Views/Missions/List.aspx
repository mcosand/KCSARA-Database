<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<IEnumerable<EventSummaryView>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

  <p><%= Html.ActionLink<MissionsController>(f => f.Yearly(DateTime.Today.Year), "Statistics ...") %> <%=Html.ActionLink<MissionsController>(f => f.MissionsKML(null,null), "Open in Google Earth...") %></p>
  <% if (Page.User.IsInRole("cdb.missioneditors") && !(bool)ViewData["filtered"]) { %><div><%= Html.ActionLink<MissionsController>(c => c.Create(), "New...") %></div> <% } %>
  <% if ((bool)ViewData["filtered"]) { %><div>Filtered List. View the unfiltered list to create a new mission.</div><% } %>
    <div style="float:left;">
    <% Html.BeginForm(); %>
  <fieldset>
    <label for="unit">Show units:</label>
    <%= Html.DropDownList("unit", "All units")%>
    <%= Html.SubmitButton("Update", "Update", new { @class = "button", style="display:block; font-size:1em;"})%>
  </fieldset>
<% Html.EndForm(); %>
</div>
  <div style="clear:both; font-size:80%; margin-bottom:.8em;">
  <% yearLinks(); %>
  </div>
  <table id="list" cellpadding="0" class="data-table" style="clear:both;">
    <thead>
      <tr>
        <% if (Page.User.IsInRole("cdb.missioneditors")) Response.Write("<th></th>"); %>
        <th>DEM</th>
        <th>Date</th>
        <th>
          Name
        </th>
        <th>Persons</th><th>Hours</th><th>Miles</th>
        <% if (Page.User.IsInRole("cdb.missioneditors")) Response.Write("<th></th>"); %>
      </tr>
    </thead>
    <tbody>
      <% foreach (EventSummaryView m in ViewData.Model)
         {
         %>
        <tr>
        <% if (Page.User.IsInRole("cdb.missioneditors")) { %>
          <td style="font-size:80%">
            <%= Html.PopupActionLink<MissionsController>(x => x.Edit(m.Id), "Edit", 300)%>
            <%= Html.PopupActionLink<MissionsController>(x => x.Delete(m.Id), "Delete", 200)%>
          </td>
          <% } %>
          <td><%: m.Number %></td>
          <td><%: m.StartTime.ToShortDateString() %></td>
          <td><%: Html.ActionLink<MissionsController>(x => x.Roster(m.Id), m.Title) %></td>
          <td class="r"><%: m.Persons %></td>
          <td class="r"><%: string.Format("{0:0.00}", m.Hours) %></td>
          <td class="r"><%: m.Miles %></td>
          <% if (Page.User.IsInRole("cdb.missioneditors")) Response.Write("<td>" + Html.ActionLink<MissionsController>(x => x.Log(m.Id), "Log") + "</td>"); %>

         </tr>
      <% } %>
    </tbody>
  </table>
  <div style="font-size:80%; margin-top:.8em">
  <% yearLinks(); %>
  </div>
<script type="text/javascript">
  $(document).ready(function() {
    $("#list").tablesorter({ widgets: ['zebra'], headers: { 3: { sorter:'link' } } });
  });
</script>

<script runat="server" language="C#">
    private void yearLinks()
    {
        //int y = (int)ViewData["maxYear"];
        //int? thisYear = (int?)ViewData["year"];
        //while (y >= (int)ViewData["minYear"])
        //{
        //    Response.Write(((thisYear.HasValue && thisYear.Value == y) ? MvcHtmlString.Create(y.ToString()) : Html.ActionLink<MissionsController>(f => f.List(y.ToString()), y.ToString())) + " ");
        //    y--;
        //}
        //Response.Write(thisYear.HasValue ? Html.ActionLink<MissionsController>(f => f.List("all"), "All") : MvcHtmlString.Create("All"));
        int? thisYear = (int?)ViewData["year"];
        int[] years = (int[])ViewData["years"];
        foreach (int y in years)
        {
            Response.Write(((thisYear.HasValue && thisYear.Value == y) ? MvcHtmlString.Create(y.ToString()) : Html.ActionLink<MissionsController>(f => f.List(y.ToString()), y.ToString())) + " ");
        }
        Response.Write(thisYear.HasValue ? Html.ActionLink<MissionsController>(f => f.List("all"), "All") : MvcHtmlString.Create("All"));
    }
</script>
</asp:Content>
