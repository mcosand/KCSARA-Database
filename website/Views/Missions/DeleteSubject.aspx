<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<SubjectGroupLink>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p><%= string.Format("Are you sure you want to remove subject {0} {1} from mission {2}?",
     Model.Subject.FirstName, Model.Subject.LastName, Model.Group.Mission.Title) %></p>
<% using (Html.BeginForm<MissionsController>(x => x.DeleteSubject(Model.Id)))
   { %>
  <input type="submit" value="Yes, Delete" />
<% } %>
</asp:Content>
