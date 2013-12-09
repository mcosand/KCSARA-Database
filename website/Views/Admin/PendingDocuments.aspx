<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>PendingDocuments</h2>
    <table class="data-table" id="docTable">
  <thead><tr><th></th><th>Title</th><th>Size</th><th>Uploaded</th><th></th></tr></thead>
  <tbody>
  <tr><td colspan="10" style="text-align:center;">Loading ...</td></tr>
  </tbody>
</table>
<script type="text/javascript">
    function thumbUrl(id) {
        return '<%=  Url.Action("DocumentThumb","Admin") %>/' + id;
    }
    function rotate(id, cw) {
        $('#thumb' + id).attr("src", '<%= Url.Content("~/Content/images/progress.gif") %>');
        $.ajax({ type: 'POST', url: '<%= Url.Action("RotateImage", "Missions") %>', data: { id: id, clockwise: cw }, dataType: 'json',
            success: function (data) {
                $('#thumb' + id).attr("src", thumbUrl(id) + '?reload=' + (new Date()).getTime());
            },
            error: function (msg) { alert(msg); }
        });
        return false;
    }
    var docForm;
    $(document).ready(function () {
        docForm = new ModelTable("#docTable", "");
        docForm.canEdit = false;
        docForm.getUrl = '<%= Url.Action("GetPendingDocuments") %>';
        docForm.deleteUrl = '<%= Url.Action("DeleteDocument", "Admin") %>';
        docForm.objectType = "document";
        docForm.dateTimeFields = ['Changed'];
        docForm.unpacker = function (data) { return data; }
        docForm.renderer = function (data, row) {
            var isImage = (data.Mime.substring(0, 5) == "image");
            var thumb = isImage ? thumbUrl(data.Id) : '<%= Url.Content("~/Content/images/mime/") %>' + data.Mime.replace("/", "_") + ".png";
            $(row).html('<td><a href="<%=  Url.Action("DownloadDoc","Admin") %>/' + data.Id + '"><img style="border:solid 1px black;" id="thumb' + data.Id + '" src="' + thumb + '" /></a>' +
                '</td><td>' + data.Title +
                '</td><td>' + Math.round((data.Size / 1024) * 10) / 10 + 'KB' +
                '</td><td>' + formatDateTime("yy-mm-dd HH:ii", data.Changed) +
                  '</td><td>' +
                  (isImage ?
                      '<a href="#" onclick="return rotate(\'' + data.Id + '\',false)">CCW</a> <a href="#" onclick="return rotate(\'' + data.Id + '\',true)">CW</a> &nbsp; ' : '') +
                  '<a href="#" onclick="docForm.DeletePrompt(\'' + data.Id + '\',$(this).parent().parent()); return false;">Delete</a>' +
               '</td>'
          )
        };
        $('#docTable').tablesorter({ widgets: ['zebra'] });
//        docForm.onUpdated = function (data) {
//            $('.wac-status').html((data.Goodness == true) ? 'Good' : (data.Goodness == false) ? 'Bad' : 'N/A');
//            $('.wac-status').css({ color: (data.Goodness == false) ? 'red' : null });
//        }
        docForm.ReloadData();
    });
</script>
</asp:Content>