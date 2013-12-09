<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Animal>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Upload Animal ID Photos</h2>
<% using (Html.BeginForm<AnimalsController>(f => f.PhotoPreview(), FormMethod.Post, new { enctype = "multipart/form-data" }))
   { %>
   <table cellpadding="0" class="data-table">
  <tbody>
  <% foreach (Animal m in Model) { %>
  <tr>
    <td><%= Html.Image(this.ResolveUrl(AnimalsController.GetPhotoOrFillInPath(m.PhotoFile)), "Badge Photo", new{style="border:2px solid black; height:6em; width:4.5em;"}) %></td>
    <td><%= m.Name %></td>
    <td><input type="file" id="f<%= m.Id %>" name="f<%= m.Id %>" /></td>
  </tr>
  <% } %>
  </tbody>
  <tfoot>
  <tr><td colspan="3" align="right">
    <input type="submit" value="Upload"/>
    </td></tr></tfoot>
  </table>
<% } %>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
