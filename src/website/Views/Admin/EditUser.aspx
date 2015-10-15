<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<AccountListRow>" %>
  <%@ Import Namespace="Kcsara.Database.Web.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <h3>
    <%: TempData["Message"] %></h3>
  <%: Html.ValidationSummary() %>
  <% Html.BeginForm(); %>
  <table>
    <tr><td>First Name:</td>
      <td>
        <%: Html.TextBoxFor(m => m.FirstName) %><%: Html.ValidationMessageFor(f => f.FirstName) %></td>
    </tr>
    <tr><td>Last Name:</td>
      <td>
        <%: Html.TextBoxFor(m => m.LastName) %><%: Html.ValidationMessageFor(f => f.LastName) %></td>
    </tr>
    <tr><td>Email address:</td>
      <td>
        <%: Html.TextBoxFor(m => m.Email) %><%: Html.ValidationMessageFor(f => f.Email) %></td>
    </tr>
    <tr><td colspan="2">
      <input type="submit" value="Update" /></td>
    </tr>
  </table>
  <% Html.EndForm(); %>
</asp:Content>