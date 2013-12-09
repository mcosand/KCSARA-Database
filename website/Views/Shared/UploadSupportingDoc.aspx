<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<string[]>" MasterPageFile="~/Views/Shared/Core.Master" %>

<asp:Content ID="main" ContentPlaceHolderID="MainContent" runat="server">
<form action="<%= Model[1] %>" method="post" enctype="multipart/form-data">
<div id="uploader">
<input type="file" id="file" name="file" />
<input type="hidden" name="target" id="target" value="<%= Model[2] %>" />
</div>
</form>
<script type="text/javascript">
  $(document).ready(function () {
    if (window.parent.document.regsiterDocUploader) window.parent.document.regsiterDocUploader($("#uploader"), <%= Model[0] %>);
  });
</script>
</asp:Content>
