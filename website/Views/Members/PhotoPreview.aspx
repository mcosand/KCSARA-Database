<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Member>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Preview Member's ID Photos</h2>
<% using (Html.BeginForm<MembersController>(f => f.PhotoCommit(null), FormMethod.Post))
   { %>
    <% foreach (Member m in Model)
       { %>
        <p><img src="<%= Html.BuildUrlFromExpression<MembersController>(f => f.PhotoData(m.Id)) %>" alt="Preview" style="border:solid 1px black; height:10em; width:7.5em" /><br />
        <input type="checkbox" id="keep" name="keep" value="<%= m.Id %>" checked="checked" />This image looks good. Use this for <b><%= m.FullName%></b>.</p>
    <% } %>
    <input type="submit" value="Commit" />
<% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
