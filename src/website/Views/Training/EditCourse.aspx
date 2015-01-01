<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Kcsar.Database.Model.TrainingCourse>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit Course: <%= Model.DisplayName %></h2>

    <%= Html.ValidationSummary() %>

    <% using (Html.BeginForm()) {%>

            <%= Html.Hidden("Id") %>
            <p>
                <label for="DisplayName">DisplayName:</label>
                <%= Html.TextBox("DisplayName", Model.DisplayName, new { @class = "input-box" })%>
                <%= Html.ValidationMessage("DisplayName", "*") %>
            </p>
            <p>
                <label for="FullName">FullName:</label>
                <%= Html.TextBox("FullName", Model.FullName, new { @class = "input-box", style="width:30em;" })%>
                <%= Html.ValidationMessage("FullName", "*") %>
            </p>
            <p>
                <label for="Categories">Categories:</label>
                <%= Html.TextBox("Categories", Model.Categories, new { @class = "input-box" })%>
                <%= Html.ValidationMessage("Categories", "*") %>
            </p>
            <p>
                <label for="WacRequired">WacRequired:</label>
                <%= Html.TextBox("WacRequired") %>
                <%= Html.ValidationMessage("WacRequired", "*") %>
            </p>
            <p>
                <label for="ShowOnCard" style="display:inline;">Show On ID Card:</label>
                <%= Html.CheckBox("ShowOnCard") %>
                <%= Html.ValidationMessage("ShowOnCard", "*") %>
            </p>
            <p>
                <label for="ValidMonths">ValidMonths (0 for no expiration):</label>
                <%= Html.TextBox("ValidMonths", Model.ValidMonths, new { @class = "input-box" })%>
                <%= Html.ValidationMessage("ValidMonths", "*") %>
            </p>
            <p>
                <label for="OfferedFrom">OfferedFrom:</label>
                <%= Html.TextBox("OfferedFrom", Model.OfferedFrom, new { @class = "input-box" })%>
                <%= Html.ValidationMessage("OfferedFrom", "*") %>
            </p>
            <p>
                <label for="OfferedUntil">OfferedUntil:</label>
                <%= Html.TextBox("OfferedUntil", Model.OfferedUntil, new { @class = "input-box" })%>
                <%= Html.ValidationMessage("OfferedUntil", "*") %>
            </p>
                <input type="submit" value="Save" class="button button-big" />

    <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

