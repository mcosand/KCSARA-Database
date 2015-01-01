<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Member>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>All Training Records for <%= Model.FullName %></h2>
  <% var dict = new ViewDataDictionary();
    dict.Add("history", true); 
    Html.RenderPartial("MemberTrainingAwards", Model.Id,dict); %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
