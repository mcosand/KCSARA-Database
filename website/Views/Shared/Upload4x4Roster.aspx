<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Views/Shared/Core.Master" %>

<asp:Content ID="main" ContentPlaceHolderID="MainContent" runat="server">
<div style="padding:.6em;">
<h2>Upload 4x4 Roster</h2>
<p>Upload Excel (*.xls or *.xlsx) file from 4x4 e-Roster:</p>

<form action="<%= Url.Action("Review4x4Roster") %>" enctype="multipart/form-data" method="post">
    <div id="uploader">
        <label for="file">Roster:</label>
        <input type="file" id="file" name="file" />
        <br />
        <br />
        <input type="submit" value="Upload" />
    </div>
</form>
</div>
</asp:Content>
