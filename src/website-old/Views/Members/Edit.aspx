<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="message"><%= TempData["message"] %></div>
    <% bool admin = (bool)ViewData["AdminEdit"]; %>
    <% using (Html.BeginForm()) { %>
      <%= Html.Hidden("Id") %>
      <table>
        <tr><td>First Name:</td><td><%= Html.TextBox("FirstName").ToString() + Html.ValidationMessage("FirstName") %></td></tr>
        <tr><td>Middle Name:</td><td><%= Html.TextBox("MiddleName").ToString() + Html.ValidationMessage("MiddleName")%></td></tr>
        <tr><td>Last Name:</td><td><%= Html.TextBox("LastName").ToString() + Html.ValidationMessage("LastName")%></td></tr>
        <tr><td>Gender:</td><td><%= Html.DropDownList("Gender").ToString() + Html.ValidationMessage("Gender")%></td></tr>
        <tr><td>Birthdate:</td><td><%= Html.TextBox("BirthDate").ToString() + Html.ValidationMessage("BirthDate")%></td></tr>

        <tr><td>DEM #:</td><td><div style="position:relative; display:inline;"><%= admin ? (Html.TextBox("DEM").ToString() +
  Html.ValidationMessage("DEM") + Html.Image("~/Content/images/progress.gif", new { id="demprogress", style="visibility:hidden; position:absolute; left:0; width:100%; top:0; height:100%;" })) : Html.Encode(Model.DEM) %></div>
            <% if (admin) { %><input id="suggest" type="button" onclick="SuggestDem();" value="Suggest" /><% } %></td></tr>
        <tr><td>WAC Role:</td><td><%= admin ? Html.DropDownList("WacLevel").ToString() : Html.Encode(Model.WacLevel)%></td></tr>
        <tr><td>WAC Role Changed:</td><td><%= admin ? (Html.DatePickerFor(f => f.WacLevelDate, new { @class="input-box", style="display:inline;"}) + Html.ValidationMessage("WacLevelDate")) : ((Model.WacLevelDate < new DateTime(1900,1,1)) ? "unknown" : string.Format("{0:yyyy-MM-dd}", Model.WacLevelDate)) %></td></tr>
        <tr><td>Background Check:</td><td><%= admin ? (Html.DatePickerFor(f => f.BackgroundDate, new { @class = "input-box", style="display:inline;" }) + Html.ValidationMessage("BackgroundDate")) : string.Format("{0:yyyy-MM-dd}",Model.BackgroundDate) %></td></tr>
        <tr><td>KCSO App Received:</td><td><%= admin ? (Html.DatePickerFor(f => f.SheriffApp, new { @class = "input-box", style="display:inline;" }) + Html.ValidationMessage("SheriffApp")) : string.Format("{0:yyyy-MM-dd}",Model.SheriffApp) %></td></tr>
        <tr><td>Photo:</td><td><%= Html.PopupActionLink<MembersController>(x => x.PhotoUpload(Model.Id.ToString()), "Change Photo", 400) %></td></tr>
        <tr><td>Card Database Key:</td><td><%= Html.TextBox("ExternalKey1").ToString() + Html.ValidationMessage("ExternalKey1") %></td></tr>
 <%--       <tr><td colspan="2">Comments:<br /><%= Html.TextArea("Comments", new { style = "width:100%" }) + Html.ValidationMessage("Comments")%></td></tr>
--%>        </table>
        <br />
        <%= Html.Hidden("NewUserGuid") %>
      <input type="submit" value="Save" />
    <% } %>
    <script type="text/javascript">
      function SuggestDem() {
        $("#suggest").disabled = true;
        $("#demprogress").css({ visibility: "visible" });
        $.post('<%= Url.Action("SuggestDem") %>', null, function(data) {
          $("#DEM").val(data);
          $("#demprogress").css({ visibility: "hidden" });
          $("#suggest").disabled = false;
        });
      }
      function updateWacDate() {
        var el = document.getElementById("WacLevelDate");
        if (el.k_updated) return;
        var d = new Date();
        el.value = d.getFullYear() + ((d.getMonth() < 9) ? '-0' : '-') + (d.getMonth() + 1) + ((d.getDate() < 10) ? '-0' : '-') + d.getDate();
        el.k_updated = true;
      }

      document.getElementById("WacLevel").onclick = updateWacDate;
</script>
</asp:Content>