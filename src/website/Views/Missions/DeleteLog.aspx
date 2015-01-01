<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<MissionLog>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p><%= string.Format("Are you sure you want to remove mission log by {0}, at time {1}?",
      (Model.Person == null) ? "[unknown]" : Model.Person.FullName,
      Model.Time)
  %></p>
<% using (Html.BeginForm<MissionsController>(x => x.DeleteLog(Model.Id)))
   { %>
  <input type="submit" value="Yes, Delete" />
<% } %>
</asp:Content>
