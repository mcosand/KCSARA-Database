<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<HandleErrorInfo>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Sorry, an error occurred while processing your request.
    </h2>
    <% if (Page.User != null && Page.User.Identity != null && Page.User.Identity.IsAuthenticated) { %>
      Report a <a href="/bugs/edit_bug.aspx">new bug</a>?
    <% } %>
    <h3>
    <%= ViewData["error"] %>
    </h3>
    <pre><%: Model.Exception.Message %></pre>
</asp:Content>
