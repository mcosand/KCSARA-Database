<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Training/Trainings.Master" Inherits="System.Web.Mvc.ViewPage<Training>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<% bool canEdit = Page.User.IsInRole("cdb.trainingeditors"); %>

<%--<% Html.RenderPartial("Training", Model); %>
--%>
<h2>Supporting Documents</h2>

<table class="data-table" id="docTable">
  <thead><tr><th>Type</th><th>Thumbnail</th><th>Filename</th><th>Size</th></tr></thead>
  <tbody>
  <tr><td colspan="10" style="text-align:center;">Loading ...</td></tr>
  </tbody>
</table>
<%= canEdit ? Html.PopupActionLink<TrainingController>(f => f.UploadDocument(Model.Id), "Upload File") : MvcHtmlString.Empty %>

<div id="docForm" title="Update Award Information" style="display:none;">
    <p class="validateTips"></p>

    <form action="#">
</form>
</div>

<script type="text/javascript">

function thumbUrl(id)
{
return '<%=  Url.Action("DocumentThumb","Training") %>/' + id;
}

function rotate(id, cw)
{
    $('#thumb'+id).attr("src", '<%= Url.Content("~/Content/images/progress.gif") %>');
    $.ajax({ type: 'POST', url: '<%= Url.Action("RotateImage", "Training") %>', data: {id: id, clockwise: cw}, dataType: 'json',
        success: function (data) {
          $('#thumb'+id).attr("src", thumbUrl(id)+'?reload='+(new Date()).getTime());
        },
        error: function(msg) { alert(msg); }
    });
    return false;
}

var docForm;
$(document).ready(function () {

  docForm = new ModelTable("#docTable", "#docForm");
  docForm.height = 430;
  docForm.width = 700;
  docForm.canEdit = false;
  docForm.getUrl = '<%= Url.Action("GetTrainingDocuments", new { id = Model.Id }) %>';
  docForm.deleteUrl = '<%= Url.Action("DeleteDocument", "Training") %>';
  docForm.objectType = "document";
  docForm.itemKey = 'Id';
  docForm.uploaders = [];
  docForm.renderer = function (data, row) {
    var isImage = (data.Mime.substring(0,5) == "image");
    var thumb = isImage ? thumbUrl(data.Id) : '<%= Url.Content("~/Content/images/mime/") %>' + data.Mime.replace("/", "_") + ".png";
    var content = '<td>' + data.Type +
                  '</td><td><a href="<%=  Url.Action("DownloadDoc","Missions") %>/' + data.Id + '"><img style="border:solid 1px black;" id="thumb'+data.Id+'" src="'+thumb + '" /></a>' +
                  '</td><td>' + data.Title +
                  '</td><td>' + Math.round((data.Size / 1024) * 10)/10 + 'KB' +
                  '</td><td>' +
<% if (canEdit) { %>
                  (isImage ?
                      '<a href="#" onclick="return rotate(\'' + data.Id + '\',false)">CCW</a> <a href="#" onclick="return rotate(\'' + data.Id + '\',true)">CW</a> &nbsp; ' : '') +
                  '<a href="#" onclick="docForm.DeletePrompt(\'' + data.Id + '\',$(this).parent().parent()); return false;">Delete</a>' +
<% } %>
                  '</td>'
    $(row).html(content);
  };
  docForm.fillForm = function (item) {
    //window.frames["uploadFrame"].location.reload();
    return true;
  };

  docForm.formParser = function (c) {
    return c;
  };

    docForm.Initialize();

  $('#addDoc').click(function () {
    var newObj = new Object();
    newObj.Course = new Object();
    newObj.Member = new Object();
    newObj.Member.Id = '<%= Model.Id %>';
    docForm.CreatePrompt(newObj);
  });

});
</script>
</asp:Content>
