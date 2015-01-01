<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div style="position:relative; margin: 2em auto; width: 667px; height: 500px; border: solid 5px Black; background-color:White">
    <img style="position: absolute; top: 15px; left: 15px; width: 637px; height: 470px; z-index: 0;"
    src="<%= Url.Content("~/Content/images/mtn1.jpg") %>" alt="" />

    <center>
          <div style="display:inline-table; position:relative; z-index:1; margin: 5em auto; border: solid 1px Black; padding: .5em; background-color: #EEEEEE">

        <div style="padding:1.5em; font-weight:bold; font-size:120%; color:Black"><%: Strings.GroupName %></div>
        <%= Html.ValidationSummary() %>
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
              <%= Html.SubmitButton("login", Strings.Login) %>
              <div style="font-size:70%; margin-top:1em"><%= Html.ActionLink<AccountController>(f => f.ResetPassword(string.Empty), "Forgotten Password") %></div>
            </td>
          </tr>
        </table>
        <% } %>
        </div>
      </center>
  </div></asp:Content>