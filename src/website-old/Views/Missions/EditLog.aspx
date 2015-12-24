<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<MissionLog>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
  <h2>
    Mission Log Entry for
    <%= Model.Mission.Title %></h2>
  <div class="message">
    <%= TempData["message"] %></div>
  <% using (Html.BeginForm())
     { %>
  <%= Html.Hidden("NewLogGuid") %>
  <%= Html.Hidden("Mission", Model.Mission.Id) %>
  <p>
    <label for="Time">
      Date and Time:</label>
    <%= Html.ValidationMessage("Time") %>
    <%= Html.TextBox("Time", Model.Time, new { @class = "input-box", style = "width:30em" }) %>
  </p>
  <p>
    <label for="Data">
      Entry:</label>
    <%= Html.ValidationMessage("Data") %>
    <%= Html.TextArea("Data", new { @class = "input-box", style = "width:30em; height:10em" })%>
  </p>
  <p>
    <label for="name_a">
      Member:</label>
    <%= Html.ValidationMessage("Person", new { style = "display:block" })%>
    <%= Html.Hidden("pid_a") %>
    <%= Html.TextBox("name_a", (Model.Person == null) ? "" : Model.Person.ReverseName, new { style = "display:inline; width:20em;" + ((Model.Person == null) ? " background-color:#ffffbb" : ""), @class = "input-box" })%>
    <%= Html.TextBox("dem_a", (Model.Person == null) ? "" : Model.Person.DEM,
                                                new { style = "width:4em;" + ((Model.Person != null) ? "" : " background-color:#ffffbb") })%>
    <%= Html.ValidationMessage("DisplayName", "*") %>
  </p>
  <input type="submit" value="Save" />
  <% } %>

  <script type="text/javascript">
    $(function() {
      $('#name_a').suggest("<%= Html.BuildUrlFromExpression<MembersController>(x => x.Suggest(null)) %>", { dataContainer: "a" });
    });
  </script>

</asp:Content>
