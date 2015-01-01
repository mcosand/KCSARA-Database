<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Mobile.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ValidationSummary() %>
<% using (Html.BeginForm())
   { %>
<label for="username">Username:</label>
<%= Html.TextBox("username")%><br />
<%= Html.ValidationMessage("username")%>

<label for="password">Password:</label>
<%= Html.Password("password")%><br />
<%= Html.ValidationMessage("password")%>
<%= Html.Hidden("rememberMe", "true") %>
<br />
<%= Html.SubmitButton(Strings.Login, Strings.Login)%>
<% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
  <title>King County SAR Login</title>
</asp:Content>
