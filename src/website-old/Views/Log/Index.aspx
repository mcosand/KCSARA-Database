<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IList<Kcsar.Database.Model.AuditLog>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<% Html.RenderPartial("LogTable", Model); %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
