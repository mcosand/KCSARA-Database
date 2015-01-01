<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<% using (Html.BeginForm()) { %>
    <h2>Settings</h2>
    <fieldset>
      <label for="coordDisplay">Coordinate Display:</label>
      <input type="radio" name="coordDisplay" value="0" <%= ((int)ViewData["coordDisplay"] == 0) ? " checked=\"checked\"" : "" %> />Decimal Degrees (dd.ddddd)<br />
      <input type="radio" name="coordDisplay" value="1" <%= ((int)ViewData["coordDisplay"] == 1) ? " checked=\"checked\"" : "" %>/>Decimal Minutes (dd mm.mmm)<br />
    </fieldset>
<%--  <fieldset>
  <h3>Global Filter</h3>
    <label for="lstUnitFilter">Show only units:</label>
    <%= Html.ListBox("lstUnitFilter") %>

    <label for="setTime">Historical time:</label>
    <%= Html.TextBox("setTime") %>
  </fieldset>
--%>  
  <label for="persist">Persistance:</label>
  <%= Html.CheckBox("persist") %> Use these settings the next time I log in.
  <input type="submit" value="Save and Close" />
<% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
