<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<UnitMembership>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
  <h2>Unit Membership for <%= ViewData.Model.Person.FullName %></h2>
  <div class="message"><%= TempData["message"] %></div>

  <% using (Html.BeginForm())
    { %>
  <%= Html.Hidden("NewMembershipGuid") %>
  <%= Html.Hidden("Person", ViewData.Model.Person.Id) %>
  <table>
    <tr>
      <td>Unit:</td>
      <td><%=
         Html.DropDownList("Unit").ToString() + Html.ValidationMessage("Unit")%>
        <%-- $TODO: A bug in MVC RC means that when you use an overload with the htmlAttributes parameter, the selected item is not propagated to the
         rendered list. When the bug is fixed, use that overload, and remove the following script. --%>
        <script type="text/javascript">$("#Unit").change(function () { GetStatusTypes(); });</script>
      </td>
    </tr>
    <tr>
      <td>Status:</td>
      <td>
        <div style="position: relative; display: inline;"><%= Html.DropDownList("Status").ToString() + Html.ValidationMessage("Status") + Html.Image("~/Content/images/progress.gif", new { id = "statusprogress", style = "visibility:hidden; position:absolute; left:0; width:100%; top:0; height:100%;" })%></div>
      </td>
    </tr>
    <tr>
      <td>Activated:</td>
      <td><%= Html.TextBox("Activated", ViewData.Model.Activated.ToShortDateString()).ToString() + Html.ValidationMessage("Activated")%>
      </td>
    </tr>
    <tr>
      <td colspan="2">Comments:<br />
        <%= Html.TextArea("Comments", new { style = "width:100%" }).ToString() + Html.ValidationMessage("Comments")%></td>
    </tr>
  </table>
  <p>
  </p>
  <input type="submit" value="Save" />
  <% } %>

  <script type="text/javascript">
    function GetStatusTypes() {
      $("#Unit").disabled = true;
      $("#statusprogress").css({ visibility: "visible" });
      var url = '<%= Url.Action("GetStatusTypes", "Units") %>' + '/' + $("#Unit").val();

  $.post(url, null, function (data) {
    var result = eval(data);
    var options = '';
    for (var i = 0; i < result.length; i++) {
      options += '<option value="' + result[i].Id + '">' + result[i].Name + '</option>';
    }
    $("#Status").html(options);

    $("#statusprogress").css({ visibility: "hidden" });
    $("#Unit").disabled = false;
  }, "json");
}
  </script>

  <% if ((bool?)ViewData["showEditWarning"] ?? false)
    { %>
  <script type="text/javascript">
    alert("*** CAUTION ***\nThis page is typically used to fix data entry errors.\n\nIf you are looking to reflect a change in a member's status, please close this window and click 'Change Status' instead of 'Edit'.");
  </script>
  <% } %>
</asp:Content>
