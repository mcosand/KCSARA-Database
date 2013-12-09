<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IList<AccountListRow>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% bool canEdit = (bool)ViewData["canedit"]; %>
<%: canEdit ? Html.ActionLink<AdminController>(x => x.CreateUser(), "Create User") : MvcHtmlString.Empty %>
<table id="t" class="data-table">
<thead>
  <tr><th></th><th>Username</th><th>Name</th><th>Last Active</th><th>Link</th><th>Email</th><th>External?</th></tr>
</thead>
<tbody>
  <% foreach (AccountListRow row in ViewData.Model) { %>
  <tr><td><%: canEdit ? Html.ActionLink<AdminController>(x => x.DeleteUser(row.Username), "X") : MvcHtmlString.Empty %>
          <%: canEdit ? Html.ActionLink<AdminController>(x => x.EditUser(row.Username), "Edit") : MvcHtmlString.Empty %></td>
    <td class="a"><%= row.Username %></td>
    <td><%: row.LastName %>, <%: row.FirstName %></td>
    <td><%: row.LastActive %></td>
    <td><div class="b"><%= row.LinkKey %></div><% if (canEdit) { %><a href="#" onclick="showPicker(this); return false;">*</a><% } %></td>
    <td><%: row.Email %></td>
    <td><%: row.ExternalSources %></td>
    <td><%: (row.IsLocked.HasValue && row.IsLocked.Value && canEdit) ? Html.ActionLink<AdminController>(f => f.UnlockAccount(row.Username), "Unlock"): MvcHtmlString.Empty %></td>
  </tr>
  <% } %>
</tbody>
<tfoot>
  <tr><th colspan="5"><%= ViewData.Model.Count %> Users</th></tr>
</tfoot>
</table>
<div id="ld" style="display:none">
  <%= Html.Hidden("pid_l") %>
  <%= Html.TextBox("link_l", "", new { style = "width:110px; font-size:80%; display:inline;", @class = "input-box" }) %>      
  <input type="button" value="save" onclick="DoAcctSave(this); return false;" />
</div>
<script>
  function DoAcctSave(button) {
    var acct = jQuery("#ld").parent().parent().find(".a").html();
    jQuery.post("<%= Html.BuildUrlFromExpression<AdminController>(x => x.LinkAccount()) %>", { acct: acct, id: document.getElementById("pid_l").value }, function(data) {
      var input = jQuery("#link_l");
      var div = jQuery("#ld");
      div.parent().find("div:first").html(document.getElementById("pid_l").value);
      div.parent().parent().find("td:eq(2)").html(input.val());
      jQuery("#pid_l").val("");
      input.val("");
      document.getElementById("ld").style.display = "none";
    });  
  }
  
  $(document).ready(function() {
    $("#t").tablesorter({ widgets: ['zebra'] });
    
    jQuery('#link_l').suggest("<%= Html.BuildUrlFromExpression<MembersController>(x => x.Suggest(null)) %>", { dataContainer: "l" });
  });
  
  function showPicker(control) {
    jQuery("#ld").insertAfter(jQuery(control));
    document.getElementById("ld").style.display = "block";
  }
</script>
</asp:Content>
