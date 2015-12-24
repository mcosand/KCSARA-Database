<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

  <div>
    <div class="noprint">
        <input type="checkbox" id="membershiphistory" /><label for="membershiphistory" style="display:inline; font-weight:normal; font-size:80%">Show history</label>
    </div>
    <table id="membermemberships" border="0" cellpadding="0" class="data-table">
    <thead>
      <tr>
        <th>Unit</th>
        <th>Status</th>
        <th>Started</th>
        <% if (Page.User.IsInRole("cdb.admins")) Response.Write("<th class='noprint'></th>"); %>
      </tr>
    </thead>
    <tbody>
    <%
      int count = 0;
      bool isActiveAny = ViewData.Model.Memberships.Any(f => f.EndTime == null && f.Status.IsActive);
      foreach (UnitMembership um in ViewData.Model.Memberships.OrderBy(x => x.Unit.DisplayName).ThenByDescending(x => x.Activated))
      { %>
      <%-- Hide all the entries with an end time. If the member is still active with any unit, also hide current "inactive" memberships --%>
      <tr class="<%= ((count++ % 2) != 0) ? "row-alternating" : "" %> <%= (um.EndTime == null && !(isActiveAny ^ um.Status.IsActive )) ? "" : "um-history" %>">
        <td><%= um.Unit.DisplayName%></td>
        <td><%= um.Status.StatusName%></td>
        <td><%= um.Activated.ToShortDateString()%></td>
        <% if (Page.User.IsInRole("cdb.admins")) { %>
        <td class="noprint"><%= Html.PopupActionLink<MembersController>(x => x.EditMembership(um.Id), "Edit")%>
            <%= Html.PopupActionLink<MembersController>(x => x.DeleteMembership(um.Id), "Delete")%></td>
        <% } %>
      </tr>    
    <% } %>
    </tbody>
    </table>
    <script type="text/javascript">
  $(document).ready(function() {
      $("#membermemberships").tablesorter({ widgets: ['zebra'], headers: { 2: { sorter: 'link' } } });
      var showHistory = function (doShow) {
          $("#membermemberships").find("tbody tr.um-history").toggle(doShow);
      }
      showHistory($("#membershiphistory").click(function () { showHistory(this.checked); }).is(':checked'));
  });
  </script>
<% if (Page.User.IsInRole("cdb.admins")) { %>
  <%= Html.PopupActionLink<MembersController>(x => x.CreateMembership(ViewData.Model.Id), Strings.ChangeAddMembership)%>
<% } %>
  </div>