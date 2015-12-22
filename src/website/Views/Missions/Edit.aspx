<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<Kcsar.Database.Model.Mission_Old>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="message"><%= TempData["message"] %></div>
    
    <% using (Html.BeginForm()) {
         object inputBox = new { @class = "input-box" }; %>
      <%= Html.Hidden("Id")%>
      <table>
        <tr><td>Title:</td><td><%= Html.EditorFor(f => f.Title, inputBox).ToString() + Html.ValidationMessage("Title")%></td></tr>
        <tr><td>Location:</td><td><%= Html.EditorFor(f => f.Location, inputBox).ToString() + Html.ValidationMessage("Location")%></td></tr>
        <tr><td>County:</td><td><%= Html.DropDownList("County").ToString() + Html.ValidationMessage("County")%></td></tr>
        <tr><td>State #:</td><td><%= Html.EditorFor(f => f.StateNumber, inputBox).ToString() + Html.ValidationMessage("StateNumber")%></td></tr>
        <tr><td>County #:</td><td><%= Html.EditorFor(f => f.CountyNumber, inputBox).ToString() + Html.ValidationMessage("CountyNumber")%></td></tr>
        <tr><td>Start Time:</td><td><%= Html.DateTimePickerFor(f => f.StartTime, inputBox) + Html.ValidationMessage("StartTime")%></td></tr>
        <tr><td>Stop Time:</td><td><%= Html.DateTimePickerFor(f => f.StopTime, inputBox) + Html.ValidationMessage("StopTime")%></td></tr>
        <tr><td>Mission Type:</td><td><%= Html.CheckBoxList("MissionType", (MultiSelectList)ViewData["MissionType"]) %></td></tr>
      </table>
      <br />
      <%= Html.Hidden("NewEventGuid") %>
      <input type="submit" value="Save" />

    <% } %>
</asp:Content>
