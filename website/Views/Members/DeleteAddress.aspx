<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<PersonAddress>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p><%= string.Format("Are you sure you want to remove {0}'s address, {1} {2} {3}?",
      ViewData.Model.Person.FullName,
      ViewData.Model.Street,
      ViewData.Model.City,
      ViewData.Model.State)
  %></p>
<% using (Html.BeginForm<MembersController>(x => x.DeleteAddress(ViewData.Model.Id)))
   { %>
  <input type="submit" value="Yes, Delete" />
<% } %>
</asp:Content>
