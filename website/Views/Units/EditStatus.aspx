<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<UnitStatus>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Status category for <%= ViewData.Model.Unit.DisplayName %></h2>
    <div class="message"><%= TempData["message"] %></div>
    
    <% using (Html.BeginForm()) { %>
      <%= Html.Hidden("NewStatusGuid") %>
      <%= Html.Hidden("Unit", ViewData.Model.Unit.Id) %>
      <table>
        <tr><td>Status Name:</td><td><%= Html.TextBox("StatusName").ToString() + Html.ValidationMessage("StatusName") %></td></tr>
        <tr><td>Is Active:</td><td><%= Html.CheckBox("IsActive").ToString() + Html.ValidationMessage("IsActive") %></td></tr>
        <tr><td>Required WAC Level:</td><td><%= Html.DropDownList("WacLevel").ToString() +  Html.ValidationMessage("WacLevel") %></td></tr>
      </table>
      <p>
      </p>
      <input type="submit" value="Save" />
    <% } %>
    

<%--<% if (false) { %>
<script type="text/javascript">
alert("*** CAUTION ***\nThis page is typically used to fix data entry errors.\n\nIf you are looking to add a new status type, please close this window and click 'Change Status' instead of 'Edit'.");
</script>
<% } %>--%>

</asp:Content>
