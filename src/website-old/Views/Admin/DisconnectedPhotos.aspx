<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<string>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Disconnected Photos</h2>
<p>These photos are saved on the database server, but are not currently linked to any members.
 If you want to save a picture, use your browser's "Right click, Save As..." function.
 Deleting these images can help save space on the server.
 </p>
<% foreach (string file in Model) { %>
  <%= Html.Image(this.ResolveUrl(MembersController.GetPhotoOrFillInPath(file)), new{style="border:2px solid black; height:12em; width:9em;"}) %>
  <%= file %><br />
<% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
