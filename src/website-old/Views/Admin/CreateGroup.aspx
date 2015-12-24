<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <h3><%= TempData["Message"] %></h3>
  <%= Html.ValidationSummary() %>
  <% Html.BeginForm(); %>
  <table>
    <tr>
      <td>Group Name:</td>
      <td><%= Html.TextBox("name") %></td>
    </tr>
    <tr>
      <td colspan="2"><input type="submit" value="Create Group" /></td>
    </tr>
  </table>
  <% Html.EndForm(); %>
</asp:Content>
