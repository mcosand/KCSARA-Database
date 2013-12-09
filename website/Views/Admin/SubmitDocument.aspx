<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Kcsar.Database.Controllers.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Submit Document</h2>
<% using (Html.BeginForm<AdminController>(f => f.SubmitDocument(), FormMethod.Post, new { enctype = "multipart/form-data" }))
   { %>
   <input type="file" id="file" name="file" />
    <input type="submit" value="Upload"/>
<% } %>

</asp:Content>