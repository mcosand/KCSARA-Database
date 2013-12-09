<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage<Kcsara.Database.Web.Model.TrainingAwardView>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Model.Member.Name %> awarded <%: Model.Course.Title %></h2>
    <label for="course">Course:</label>
    <div id="course"><%: Html.ActionLink<TrainingController>(f => f.Current(Model.Course.Id, null, false), Model.Course.Title) %></div>

    <label for="awardcompleted">Completed:</label>
    <div id="awardcompleted"><%: Model.Completed %></div>

    <label for="awardexpiry">Expires:</label>
    <div id="awardexpiry"><%= Model.Expires %></div>

    <label for="awardcomments">Comments:</label>
    <textarea readonly="readonly" cols="60" id="awardcomments" rows="6"><%= Model.Comments %></textarea>
    <br /><br />
    <strong>Documentation:</strong>
    <div id="awardDocs">
    <% foreach (DocumentView doc in (DocumentView[])ViewData["docs"])
       {
         Response.Write("<div>");
         Html.RenderPartial("SupportingDoc", doc);
         Response.Write("</div>");
       } %>
    </div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
<style type="text/css">
.display-field { margin-bottom: 1em; }
.display-label { font-weight:bold; }
</style>
</asp:Content>

