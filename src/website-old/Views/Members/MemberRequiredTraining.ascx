<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Guid>" %>
<table class="data-table" id="wacTable">
  <thead><tr><th>Course</th><th>Completed</th><th>Expires</th></tr></thead>
  <tbody>
  <tr><td colspan="10" style="text-align:center;">Loading ...</td></tr>
  </tbody>
</table>
<script type="text/javascript">
var wacForm;
$(document).ready(function () {
  wacForm = new ModelTable("#wacTable", "");
  wacForm.canEdit = false;
  wacForm.getUrl = '<%= Url.Action("GetMemberExpirations", "Training", new { id = Model }) %>';
  wacForm.objectType = "expiration";
  wacForm.unpacker = function (data) { return data.Expirations; }
  wacForm.renderer = function (data, row) {
    $(row).html('<td>' + data.Course.Title +
               '</td><td>' + data.Completed +
               '</td><td class="exp_' + data.Status + '">' + data.ExpiryText +
               '</td>'
          )
  };
  $('#wacTable').tablesorter({ widgets: ['zebra'] });
  wacForm.onUpdated = function (data) {
    $('.wac-status').html((data.Goodness == true) ? 'Good' : (data.Goodness == false) ? 'Bad' : 'N/A');
    $('.wac-status').css({ color: (data.Goodness == false) ? 'red' : null });
  }
  wacForm.ReloadData();
});
</script>