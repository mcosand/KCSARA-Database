<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Animal>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>
    Detail</h2>
  <div style="float:left;"><%= Html.Image(this.ResolveUrl(AnimalsController.GetPhotoOrFillInPath(Model.PhotoFile)), "Badge Photo", new{style="border:2px solid black; height:12em; width:9em;"}) %></div>
  <p>
    Name:
    <%= Html.Encode(Model.Name) %>
  </p>
  <p>
    Type:
    <%= Html.Encode(Model.Type) %>
  </p>
  <p>
    DemSuffix:
    <%= Html.Encode(Model.DemSuffix) %>
  </p>
  <p>
    Comments:
    <%= Html.Encode(Model.Comments) %>
  </p>
  <%= Html.PopupActionLink<AnimalsController>(x => x.Edit(Model.Id), "Edit", 400) %>
  <%= Html.PopupActionLink<AnimalsController>(x => x.PhotoUpload(Model.Id.ToString()), "Photo", 400) %>
  <div style="clear:both;"> </div>
  <p>
    Owners are those that might be responsible for the animal's handling on a mission.
    These members may sign the animal into the roster. The animal will inherit the DEM#
    of the current primary owner, followed by the animals "DEM Suffix".</p>
  <table id="t" class="data-table">
    <thead>
      <tr>
        <th>
          Owner
        </th>
        <th>
          Primary?
        </th>
        <th>
          Starting
        </th>
        <th>
          Ending
        </th>
        <% if (Page.User.IsInRole("cdb.admins")) Response.Write("<th></th>"); %>
      </tr>
    </thead>
    <tbody>
      <% foreach (AnimalOwner ownerRow in Model.Owners)
         { %>
      <tr>
        <td>
          <%= Html.ActionLink<MembersController>(f => f.Detail(ownerRow.Owner.Id), ownerRow.Owner.ReverseName) %></td>
        <td>
          <%= ownerRow.IsPrimary %></td>
        <td>
          <%= ownerRow.Starting %></td>
        <td>
          <%= ownerRow.Ending %></td>
        <% if (Page.User.IsInRole("cdb.admins"))
           { %>
        <td>
          <%= Html.PopupActionLink<AnimalsController>(x => x.EditOwner(ownerRow.Id), "Edit", 320) %>
          <%= Html.PopupActionLink<AnimalsController>(x => x.DeleteOwner(ownerRow.Id), "Delete", 200) %>
        </td>
        <% } %>
      </tr>
      <% } %>
    </tbody>
  </table>
  <% if (Page.User.IsInRole("cdb.admins"))
     { %>
  <%= Html.PopupActionLink<AnimalsController>(x => x.CreateOwner(Model.Id), "Add New Owner", 320) %>
  <% } %>
  
  <p>Mission Response:</p>
  <table id="rosterevents" border="0" cellpadding="0" class="data-table">
    <thead>
      <tr>
        <th>
          DEM
        </th>
        <th>
          Start Time
        </th>
        <th>
          Title
        </th>
        <th>
          Time In
        </th>
        <th>
          Time Out
        </th>
        <th>
          Hours
        </th>
      </tr>
    </thead>
    <tbody>
      <%
         foreach (MissionRoster row in Model.MissionRosters.OrderByDescending(x => x.MissionRoster.TimeIn).Select(f => f.MissionRoster))
         {
           IRosterEvent evt = row.GetEvent();      
      %>
      <tr>
        <td>
          <%= evt.StateNumber %></td>
        <td>
          <%= evt.StartTime.ToString("MM-dd-yy") %></td>
        <td>
          <%= Html.ActionLink<MissionsController>(f => f.Roster(evt.Id), row.GetEvent().Title) %>
        </td>
        <%-- Time In and Time Out. In 2400 military time. If the time is not on the same date as the event start,
      prepend the difference in days. (ex: Mission starts on Sunday, I sign in that night, and out the next day.
      The times would be printed "2300" "1+0900" --%>
        <td class="r">
          <%=
            ((row.TimeIn.Value.Date != evt.StartTime.Date)
                ? ((row.TimeIn.Value.Date - evt.StartTime.Date).Days.ToString() + "+")
                : "")
            + row.TimeIn.Value.ToString("HHmm") %></td>
        <td class="r">
          <%=
            ((row.TimeOut.HasValue && row.TimeOut.Value.Date != evt.StartTime.Date)
                  ? (row.TimeOut.Value.Date - evt.StartTime.Date).Days.ToString() + "+"
                  : "")
            + ((row.TimeOut.HasValue)
                  ? row.TimeOut.Value.ToString("HHmm") 
                  : "")
          %></td>
        <td class="r">
          <%= (row.TimeOut.HasValue) ? (row.TimeOut.Value - row.TimeIn.Value).TotalHours.ToString("0.0") : "" %>
        </td>
      </tr>
      <% } %>
    </tbody>
    <tfoot>
      <tr>
        <th class="l" colspan="5">
          <%= string.Format("{0} Missions", Model.MissionRosters.GroupBy(x => x.MissionRoster.GetEvent().Id).Count()) %>
        </th>
        <th class="r">
          <%= Model.MissionRosters.Sum(x => x.MissionRoster.TimeOut.HasValue ? (x.MissionRoster.TimeOut.Value - x.MissionRoster.TimeIn.Value).TotalHours : 0).ToString("0.0") %>
        </th>
      </tr>
    </tfoot>
  </table>
  
  <script type="text/javascript">
    $(document).ready(function() {
      $("#rosterevents").tablesorter({ widgets: ['zebra'], headers: { 2: { sorter: 'link'}} });
    });
  </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
