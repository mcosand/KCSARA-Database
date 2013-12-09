<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Training>" MasterPageFile="~/Views/Shared/Core.Master" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="main" ContentPlaceHolderID="MainContent" runat="server">
<div style="padding:.6em;">
<h2>Upload Document for <%: Model.StateNumber %> <%: Model.Title %></h2>
<% Html.BeginForm<TrainingController>(f => f.UploadDocument(Model.Id), FormMethod.Post, new { enctype = "multipart/form-data" }); %>
<div id="uploader">
<label for="file">Document:</label>
<input type="file" id="file" name="file" />
<br />
<br />
<input type="submit" value="Upload" />
</div>
<% Html.EndForm(); %>
</div>
<script type="text/javascript">
  $(document).ready(function () {
  });
</script>
</asp:Content>
