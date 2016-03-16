<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <h2>
    Training</h2>
  <ul>
    <li>
      <%= Html.ActionLink<TrainingController>(x => x.CourseList(null,null,null,null), "Course List") %></li>
    <li>
      <%= Html.ActionLink<TrainingController>(x => x.List(null), "Past Trainings (Rosters)") %></li>
    <li><%= Html.ActionLink<TrainingController>(x => x.RequiredTrainingReport(), "Required Training Report")  %></li>
    <li><%= Html.ActionLink<TrainingController>(x => x.CoreCompReport(null), "Core Competency Report (all members)")  %></li>
    <% if (User.IsInRole("cdb.trainingeditors")) { %>
      <li><%= Html.ActionLink<TrainingController>(x => x.Rules(), "View Training Equivalencies") %></li>
      <li><%= Html.ActionLink<TrainingController>(x => x.UploadKcsara(), "Upload KCSARA Exam Results") %></li>
      <li><%= Html.ActionLink<TrainingController>(x => x.RecalculateAwards(null), "Recalculate Awarded Trainings") %> - Slow (Takes minutes).</li>
    <% } %>
    <% if ((bool)ViewData["showESAR"])
       { %>
    <li style="margin-top:1em;"><%= Html.ActionLink<TrainingController>(x => x.IstTrainingReport(null,null), "SAR IST Training Report")%></li>
    <li style="margin-top:1em;"><%= Html.ActionLink<TrainingController>(x => x.SpartTrainingReport(null,null), "SPART Training Report")%></li>
    <li style="margin-top:1em;"><%= Html.ActionLink<TrainingController>(x => x.EsarTrainingReport(), "ESAR Training Report")%></li>

    <li>Eligible Emails: 
<%--     <%= Html.ActionLink<TrainingController>(x => x.EligibleEmails(null, new Guid("c21822d9-fafb-4ad8-a30a-168b2f139e82"), new[] { new Guid("ce320813-6641-413e-b811-1ccc4f112cf2") }), "Course B")%>
     <%= Html.ActionLink<TrainingController>(x => x.EligibleEmails(null, new Guid("1a819ef3-a281-4b4e-a68b-3f920329d317"), new[] { new Guid("c21822d9-fafb-4ad8-a30a-168b2f139e82") }), "Course C")%>
     <%= Html.ActionLink<TrainingController>(x => x.EligibleEmails(null, new Guid("d10f2fb4-c920-4ef0-8b33-a2ce97416b2b"), new[] { new Guid("1a819ef3-a281-4b4e-a68b-3f920329d317") }), "Course I")%>
     <%= Html.ActionLink<TrainingController>(x => x.EligibleEmails(null, new Guid("2728797e-f4d5-4b29-bf69-076a9c5e0ae7"), new[] { new Guid("d10f2fb4-c920-4ef0-8b33-a2ce97416b2b") }), "Course II")%>
--%>
     <a href="<%= @Url.Action("EligibleEmails") %>/?eligibleFor=c21822d9-fafb-4ad8-a30a-168b2f139e82&haveFinished=ce320813-6641-413e-b811-1ccc4f112cf2">Course B</a>
     <a href="<%= @Url.Action("EligibleEmails") %>/?eligibleFor=1a819ef3-a281-4b4e-a68b-3f920329d317&haveFinished=c21822d9-fafb-4ad8-a30a-168b2f139e82">Course C</a>
     <a href="<%= @Url.Action("EligibleEmails") %>/?eligibleFor=d10f2fb4-c920-4ef0-8b33-a2ce97416b2b&haveFinished=1a819ef3-a281-4b4e-a68b-3f920329d317">Course I</a>
     <a href="<%= @Url.Action("EligibleEmails") %>/?eligibleFor=2728797e-f4d5-4b29-bf69-076a9c5e0ae7&haveFinished=d10f2fb4-c920-4ef0-8b33-a2ce97416b2b">Course II</a>
     <a href="<%= @Url.Action("EligibleEmails") %>/?eligibleFor=e7626c46-a494-4954-ab68-fe46c1a680d4&haveFinished=2728797e-f4d5-4b29-bf69-076a9c5e0ae7&haveFinished=6D826AB0-0802-47AA-9C4E-833B39F863EC&haveFinished=7F104382-FC66-4A8A-8A38-A72DEF66716A">Course III</a>
    </li>
    <% } %>
  </ul>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
