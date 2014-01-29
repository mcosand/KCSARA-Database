<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<Member>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <% Member m = ViewData.Model; %>

  <div id="card" style="padding: 2px; width: 25em; border: solid 1px black; ">
    <div class="wac_<%= m.WacLevel %>" style="font-weight:bold; text-align:center; text-transform:uppercase; margin-bottom:.3em"><%= (bool)ViewData["isApplicant"] ? "Applicant" : m.WacLevel.ToString() %></div>
    <div style="margin:.2em .2em 0 0; position:relative;">
      <div style="float:left;"><%= Html.Image(this.ResolveUrl(MembersController.GetPhotoOrFillInPath(m.PhotoFile)), "Badge Photo", new{style="border:2px solid black; height:12em; width:9em;"}) %></div>
      <div style="float:left; white-space:nowrap; margin-left:.4em; height:12em;">
        <div id="cardname" style="font-weight:bold; font-size:1.2em;"><%= m.FirstName + " " + m.LastName %></div>
<!--        <table>
        <tr><td style="text-align:right">DEM#</td><td><%= m.DEM %></td></tr>
        <% if (m.BirthDate.HasValue){ %>
          <tr><td style="text-align:right">BirthDate</td><td><%= m.BirthDate.Value.ToShortDateString() %></td></tr>
        <% } %>
        </table>
        -->
        DEM#: <%= m.DEM %><br />
        <% if (m.BirthDate.HasValue){ %>
          Birthday: <%= m.BirthDate.Value.ToShortDateString() %><br />
        <% } %>
        
        <% if (m.IsYouth.HasValue && m.IsYouth.Value)
           { %>
          Youth Member<br />
        <% } %>
          <% if ((bool)ViewData["CanEditAdmin"])
     { %>
     <%: string.IsNullOrEmpty(m.Username) ? MvcHtmlString.Empty : MvcHtmlString.Create(m.Username) %><br />
  <% } %>

        <div style="position:absolute; bottom:.2em; right:.2em;">
        Background Check: <%: m.BackgroundText %><br />
        <% if ((bool)ViewData["IsUser"]) { %>WAC Expiration: <span class="wac-status">Loading...</span><br /> <% } %>
        <%= (bool)ViewData["CanEditPhoto"] ? Html.PopupActionLink<MembersController>(x => x.PhotoUpload(m.Id.ToString()), "Photo", 400) : MvcHtmlString.Empty %>
        <%= (bool)ViewData["CanPrintBadge"] ? Html.ActionLink<MembersController>(x => x.Badges(m.Id.ToString()), "Badges").ToString() : "" %>
        <%= (bool)ViewData["CanEditMember"] ? Html.PopupActionLink<MembersController>(x => x.Edit(m.Id), "Edit", 300).ToString() : "" %>
        <%= (bool)ViewData["CanEditAdmin"] ? Html.ActionLink<MembersController>(x => x.Delete(m.Id), Strings.ActionDelete).ToString() : "" %>
        </div>
      </div>
      <div style="clear:both"></div>
    </div>
  </div>
  <% if ((bool)ViewData["isApplicant"]) Html.RenderPartial("_Application", m); %>

  <% if (!((bool)ViewData["isApplicant"] && Model.Memberships.Count == 0 && !(bool)ViewData["CanEditMember"])) { %>
  <h2>Unit Membership:</h2>
  <% Html.RenderPartial("MemberMemberships", m); %>
  <% } %>
        
  <h2>Addresses:</h2>
  <% Html.RenderPartial("MemberAddresses", m); %>

  <h2>Contact Information:</h2>
  <% Html.RenderPartial("MemberContacts", m); %>

  <h2>Medical Information</h2>
  <% Html.RenderPartial("_MemberMedical", m); %>
  
    <% if (Model.Animals.Count > 0)
     { %>
<%--    <h2>Animals:</h2>
    <table id="tanimals" border="0" cellpadding="0" class="data-table">
      <thead><tr><th>Name</th><th>Type</th></tr></thead>
      <tbody>
        <% foreach (Animal a in Model.Animals.Select(f => f.Animal))
           { %>
        <tr><td><%= Html.ActionLink<AnimalsController>(f => f.Detail(a.Id), a.Name) %></td><td><%= a.Type%></td></tr>
        <% } %>
      </tbody>
    </table>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#tanimals").tablesorter({ widgets: ['zebra'], headers: { 1: { sorter: 'link'}} });
        });
    </script>
--%>  <% } %>

  <div>
  <% if ((bool)ViewData["IsUser"]) {  %>
    <div style="float:left; margin-right:8em;">
    <h2>Training Awards:</h2>
    <% Html.RenderPartial("MemberTrainingAwards", Model.Id); %>
    <%: Page.User.IsInRole("cdb.trainingeditors") ? Html.PopupActionLink<TrainingController>(f => f.RecalculateAwards(Model.Id), "Recalculate Awards") : MvcHtmlString.Empty %><br />
    <%= Html.ActionLink<MembersController>(f => f.Awards(m.Id), "View all awards") %>
    </div>
    <% } %>
    <div style="float:left;">
    <h2>Required Training:</h2>
    <% Html.RenderPartial("MemberRequiredTraining", Model.Id); /*ViewData["RequiredStatus"]);*/ %>
    </div>
    <div style="clear:both;"></div>
  </div>

  <% if ((bool)ViewData["IsUser"]) {  %>
  <div style="clear: both">
  <h2>Mission Response:</h2>
  <table id="missionList" border="0" cellpadding="0" class="data-table">
    <thead>
      <tr><th>DEM</th><th>Start Time</th><th>Title</th><th>Unit</th><th>Hours</th><th>Miles</th></tr>
   </thead>
   <tbody data-bind="foreach: Rows">
     <tr>
       <td data-bind="text:Mission.StateNumber"></td>
       <td data-bind="text:Mission.StartTime.format('YY-MM-dd HH:mm')"></td>
       <td data-bind="text:Mission.Title"></td>
       <td></td>
       <td data-bind="text:Hours"></td>
       <td data-bind="text:Miles"></td>
     </tr>
   </tbody>
  </table>
  </div>
    <% } %>

  <div>
  <h2>Past Training:</h2>
  <% Html.RenderPartial("RosterOfEvents", new RosterRowsContext { Type = RosterType.Training, Rows = m.TrainingRosters.Cast<IRosterEntry>() }); %>
  </div>

  <script type="text/javascript">
    var MissionListModel = function () {
      var self = this;
      this.Rows = ko.observableArray();
      this.IsLoading = ko.observable(false);
      this.Reload = function () {
        self.IsLoading(true);
        $.getJSON("<%= Url.RouteUrl(Kcsara.Database.Web.Areas.Missions.api.RosterApiController.RouteName, new { httproute = "", action = "GetMemberResponses", id = Model.Id }) %>")
                        .done(function (data) {
                          $.each(data, function (i,x) {
                            x.Mission.StartTime = new moment(x.Mission.StartTime);
                          });
                          //mapMoment(data, ['StartTime']);
                          self.Rows(data);
                        })
                        .always(function () { self.IsLoading(false); });
      }
    }
    $(document).ready(function () {
      var model = new MissionListModel();
      ko.applyBindings(model, $('#missionList')[0]);
      model.Reload();
    });
  </script>

</asp:Content>
