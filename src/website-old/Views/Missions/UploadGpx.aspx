<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Mission_Old>" MasterPageFile="~/Views/Shared/Core.Master" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>

<asp:Content ID="main" ContentPlaceHolderID="MainContent" runat="server">
<div style="padding:.6em;">
<h2>Upload Track for <%: Model.StateNumber %> <%: Model.Title %></h2>
<% Html.BeginForm<MissionsController>(f => f.UploadGpx(Model.Id), FormMethod.Post, new { enctype = "multipart/form-data" }); %>
<div id="uploader">
<%= Html.ValidationSummary() %>
<label for="file">GPX File:</label>
<input type="file" id="file" name="file" />
<br />
<label for="kind">Track Type:</label>
<select id="kind" name="kind"><option value="teamtrk">Team Track</option></select>
<label for="description">Description:</label>
<input type="text" id="description" name="description" />
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
