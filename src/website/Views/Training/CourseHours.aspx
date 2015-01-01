<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<TrainingCourseHoursView>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsar.Database" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Hours for Course <%: Model.CourseName %></h2>
  <table class="data-table" id="table">
  <thead><tr><th>Name</th><th># Rosters</th><th>Hours</th></tr></thead>
  <tbody>
  <tr><td colspan="3" style="text-align:center;">Loading ...</td></tr>
  </tbody>
  </table>

  <script type="text/javascript">
      var href = window.location.href;
      var shortRef = href.substr(0, href.toLowerCase().indexOf('/coursehours/') + 1);

      function handleError(request, status, error) {
          if (request.status == 403 && request.responseText == 'login') {
              $('#loginForm').dialog('open');
              $('#sl_username').focus();
              $('#loginForm').dialog('option', 'position', 'center');
          }
          else {
              alert('Error submitting request:\n  ' + request + '::' + status + '::' + error);
          }
      }

      function appendRow(data, refreshSort) {
          row = $('<tr id="i' + data.Id + '"></tr>');
          renderRow(data, row);
          $('#table > tbody:last').append(row);

          if (refreshSort) {
              $("table").trigger("update");
              $("table").trigger("applyWidgets");
          }
      }

      function renderRow(data, row) {
          $(row).html('<td><a href="<%= Url.Action("Detail", "Members") %>/' + data.Person.Id + '#rostereventsTraining">' + ifDefined(data.Person.Name) +
  '</a></td><td>' + data.Count +
  '</td><td>' + data.Hours +
          //'</td><td>'+waypointLocation(data)+
          //'</td><td>'+ (isNaN(data.Len) ? "" : (data.Len/1609.344).toFixed(2)) +
          //'</td><td>'+((data.T == undefined) ? "" : formatDateTime("yy-mm-dd HH:ii", data.T)) +
  '</td>'
  );
      }

      function reloadData() {
          $.ajax({ type: 'POST', url: shortRef + 'GetCourseHours', data: { id: '<%: Model.CourseId %>', begin: '<%= string.Format("{0:yyyy-MM-dd}", Model.Begin) %>' }, dataType: 'json',
              success: function (data) {
                  $('#table tbody tr').remove();
                  for (i in data) {
                      var item = data[i];
                      fixTime(item);
                      appendRow(data[i], false);
                  }
                  $("table").trigger("update");
                  $("#table").trigger("applyWidgets")
              },
              error: handleError
          });
      }

      $(document).ready(function () {
          $("#table").tablesorter({ widgets: ['zebra'] });
          reloadData();
      });

      function ifDefined(value) { return (value == undefined) ? "" : value; }
      function ifNumeric(value) { return isNaN(value) ? "" : value; }
      function fixTime(item) { if (item.T != undefined) item.T = new Date(parseInt(/\/Date\((\d+).*/.exec(item.T)[1])); }

  </script>
</asp:Content>
