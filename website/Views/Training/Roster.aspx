<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Training/Trainings.Master" Inherits="System.Web.Mvc.ViewPage<ExpandedRowsContext>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<% Html.RenderPartial("EventRoster", Model); %>
</asp:Content>
