<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <h3><%= TempData["Message"] %></h3>
  <%= Html.ValidationSummary() %>
  <% Html.BeginForm(); %>
  <table>
    <tr>
      <td>First Name:</td>
      <td><%= Html.TextBox("first") %></td>
    </tr>
    <tr>
    <td>Last Name:</td>
    <td><%= Html.TextBox("last") %></td></tr>
    <tr>
      <td>Username:</td>
      <td><%= Html.TextBox("username") %></td>
    </tr>
    <tr>
      <td>Email address:</td>
      <td><%= Html.TextBox("email") %></td>
    </tr>
    <tr>
      <td colspan="2"><input type="submit" value="Create" /></td>
    </tr>
  </table>
  <% Html.EndForm(); %>
</asp:Content>
