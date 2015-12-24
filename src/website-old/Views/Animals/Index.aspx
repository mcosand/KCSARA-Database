<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<AnimalListRow>>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <table id="the-table" class="data-table">
    <thead>
      <tr>
        <th>
          Name
        </th>
        <th>Animal</th>
        <th>Primary Owner</th>
        <th>Inactive</th>
        <% if (User.IsInRole("cdb.admins"))
           { %><td>&nbsp;</td><% } %>
      </tr>
    </thead>
    <tbody>
      <% foreach (AnimalListRow a in ViewData.Model.OrderBy(f => f.PrimaryOwnerName))
         {%>
        <tr>
        <td><%= Html.ActionLink<AnimalsController>(x => x.Detail(a.Animal.Id), a.Animal.Name) %></td>
        <td><%= a.Animal.Type %></td>
        <td><%= a.PrimaryOwnerName %></td>
        <td><%= a.ActiveUntil %></td>
       <%--   <td><input type="checkbox" name="id" value="<%= u.Id %>" onchange="changeSelectedHandler(this)" /></td> --%>
       <%--   <td><%= Html.ActionLink<UnitsController>(x => x.Detail(u.Id), u.DisplayName) %></td> --%>
         <% if (User.IsInRole("cdb.admins")) { %>
         <td><%= Html.PopupActionLink<AnimalsController>(x => x.Edit(a.Animal.Id), Strings.ActionEdit, 400) %>
         <%= Html.PopupActionLink<AnimalsController>(x => x.Delete(a.Animal.Id), Strings.ActionDelete, 200) %>
         </td>
  <% } %>
         </tr>
      <% } %>
    </tbody>
  </table>
  <% if (User.IsInRole("cdb.admins")) { %>
    <%= Html.PopupActionLink<AnimalsController>(x => x.Create(), Strings.CreateAnimal, 400) %>
  <% } %>
<script type="text/javascript">
  $(document).ready(function() {
    $("#the-table").tablesorter({ widgets: ['zebra'], headers: { 1: { sorter: 'link'}} });
  });
</script>    

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
