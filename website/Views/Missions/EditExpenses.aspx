<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<MissionDetails>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
<% Html.BeginForm(); %>
<h2>Equipment Lost or Damaged / Extraordinary Expenses: <%= Model.Mission.Title %></h2>
<%= Html.EditorFor(f => f.EquipmentNotes) %>
<%= Html.ValidationMessage("EquipmentNotes") %>
<input type="submit" value="Save" />
<% Html.EndForm(); %>
</asp:Content>
