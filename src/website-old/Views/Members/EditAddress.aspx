<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<PersonAddress>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Address for <%= ViewData.Model.Person.FullName %></h2>
    <div class="message"><%= TempData["message"] %></div>
    <% using (Html.BeginForm()) { %>
      <%= Html.Hidden("NewAddressGuid") %>
      <%= Html.Hidden("Person", ViewData.Model.Person.Id) %>
      
      <%= Html.ValidationSummary() %>
      
      <table>
      <tr><td>Type:</td><td><%= Html.DropDownList("Type").ToString() + Html.ValidationMessage("Type")%></td></tr>
      <tr><td>Street:</td><td><%= Html.TextBox("Street").ToString() + Html.ValidationMessage("Street")%></td></tr>
      <tr><td>City:</td><td><%= Html.TextBox("City").ToString() + Html.ValidationMessage("City")%></td></tr>
      <tr><td>State:</td><td><%= Html.DropDownList("State").ToString() + Html.ValidationMessage("State")%></td></tr>
      <tr><td>ZIP:</td><td><%= Html.TextBox("Zip").ToString() + Html.ValidationMessage("Zip")%></td></tr>
      
      </table>
      <input type="submit" value="Save" />
    <% } %>
</asp:Content>
