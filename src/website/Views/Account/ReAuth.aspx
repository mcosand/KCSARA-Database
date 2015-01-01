<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <center>
        <%= Html.ValidationSummary()%>
        <% using (Html.BeginForm())
           { %>
        <table>
          <tr>
            <td>Username:</td>
            <td>
              <%= Html.TextBox("username")%><br />
              <%= Html.ValidationMessage("username")%>
            </td>
          </tr>
          <tr>
            <td>Password:</td>
            <td>
              <%= Html.Password("password")%><br />
              <%= Html.ValidationMessage("password")%>
            </td>
          </tr>
          <tr style="display:none;">
            <td></td>
            <td>
              <%= Html.CheckBox("rememberMe")%>
              Remember me</td>
          </tr>
          <tr>
            <td></td>
            <td>
              <%= Html.SubmitButton(Strings.Login, Strings.Login)%>
          </tr>
        </table>
        <% } %>
      </center>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
  <title>King County SAR Login</title>
</asp:Content>
