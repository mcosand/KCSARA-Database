<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<DocumentView>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<div style="padding:.25em; font-size:.75em;">
<img style="float:left; height:3em; width:2.2em; border-width:0; margin-right:.3em;" />
<strong><%: Html.ActionLink(Model.Title, "DownloadDoc", new { id = Model.Id }, new { target="_blank" }) %></strong><br />
Size: <%: Model.Size / 1024 %>K
</div>
<% if (ViewData["notify"] != null)
    { %>
    <script src="<%= this.ResolveUrl("~/Content/script/core.js") %>"></script>
    <script type="text/javascript">
      $(document).ready(function () {
        if (window.parent.document.notifyDocUpload) window.parent.document.notifyDocUpload('<%= ViewData["notify"] %>');
      });
</script>
<% } %>
