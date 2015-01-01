<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<div>
<table border="0" cellpadding="0" class="data-table" id="address_table">
<% int count = 0;
  foreach (PersonAddress address in ViewData.Model.Addresses)
   { %>
    <tr class="<%= ((count++ % 2) != 0) ? "row-alternating" : "" %>">
      <td style="font-weight:bold"><%= address.Type %></td>
      <td><%= address.SimpleText %></td>
      <% if (ViewData["CanEditSelf"] != null && (bool)ViewData["CanEditSelf"])
         { %>
        <td><%= Html.PopupActionLink<MembersController>(x => x.EditAddress(address.Id), "Edit", 300)%>
            <%= Html.PopupActionLink<MembersController>(x => x.DeleteAddress(address.Id), "Delete", 300)%></td>
       <% } %>
    </tr>
<% } %>
</table>
<% if (ViewData["CanEditSelf"] != null && (bool)ViewData["CanEditSelf"])
   { %>
  <%= Html.PopupActionLink<MembersController>(x => x.CreateAddress(ViewData.Model.Id), "Add Address", 300)%>
<% } %>

</div>