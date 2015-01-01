<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="resetPasswordContent" ContentPlaceHolderID="MainContent" runat="server">
  <h2>
    Reset Password</h2>
  <p>
    Use the form below to reset your password.
  </p>
  <% using (Html.BeginForm())
     { %>
  <div>
    <table>
      <tr>
        <td>Username:</td>
        <td>
          <%= Html.TextBox("id") %>
          <%= Html.ValidationMessage("id") %>
        </td>
      </tr>
      <tr>
        <td></td>
        <td>
          <input type="submit" value="Reset Password" /></td>
      </tr>
    </table>
  </div>
  <% } %>
</asp:Content>
