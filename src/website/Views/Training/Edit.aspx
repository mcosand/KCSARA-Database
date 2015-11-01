<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="ViewPage<Training>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="message"><%= TempData["message"] %></div>
    
    <% using (Html.BeginForm()) { %>
      <%= Html.Hidden("Id")%>
      <table>
        <tr><td>Title:</td><td><%= Html.EditorFor(f => f.Title).ToString() + Html.ValidationMessage("Title")%></td></tr>
        <tr><td>Location:</td><td><%= Html.EditorFor(f => f.Location).ToString() + Html.ValidationMessage("Location")%></td></tr>
        <tr><td>County:</td><td><%= Html.EditorFor(f => f.County).ToString() + Html.ValidationMessage("County") %></td></tr>
        <tr><td>State #:</td><td><%= Html.EditorFor(f => f.StateNumber).ToString() +Html.ValidationMessage("StateNumber")%></td></tr>
        <tr><td>Start Time:</td><td><%= Html.EditorFor(f => f.StartTime).ToString() + Html.ValidationMessage("StartTime") %></td></tr>
        <tr><td>Stop Time:</td><td><%= Html.EditorFor(f => f.StopTime).ToString() + Html.ValidationMessage("StopTime") %></td></tr>
        <tr><td>Host Units:</td><td><%= Html.ListBox("HostUnits") %></td></tr>
        <tr><td>Courses Offered:</td><td><%= Html.ListBox("OfferedCourses")%></td></tr>
      </table>
      <br />
      <%= Html.Hidden("NewEventGuid") %>
      <input type="submit" value="Save" />

    <% } %>
</asp:Content>
