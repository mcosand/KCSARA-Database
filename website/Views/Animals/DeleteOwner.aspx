<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<AnimalOwner>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<p>Are you sure you want to remove <%= Model.Owner.FullName %> as an owner of <%= Model.Animal.Name %> ?</p>
<% using (Html.BeginForm())
   { %>
   <%= Html.Hidden("Id") %>
  <input type="submit" value="<%: Strings.ConfirmDelete %>" />
<% } %>
</asp:Content>
