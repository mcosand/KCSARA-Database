<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Kcsar.Database.Model.SubjectGroup>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%
         foreach (SubjectGroupLink link in Model.SubjectLinks.OrderBy(f => f.Number))
         { %>
           <div id="s<%= link.Subject.Id %>" style="float: left; padding: .3em; margin: 0 .3em; border: solid 1px black;">
           <%
             ViewData["prefix"] = string.Format("{0}: ", link.Number);
             Html.RenderPartial("Subject", link.Subject);
           %>
           <% if (Page.User.IsInRole("cdb.missioneditors")) { %>
           <%= Html.PopupActionLink<MissionsController>(f => f.EditSubject(link.Subject.Id), "Edit") %>
           <%= Html.PopupActionLink<MissionsController>(f => f.DeleteSubject(link.Id), "Delete") %>
           <br />
           <%= Html.ActionLink<MissionsController>(f => f.MoveSubjectOrder(link.Id, -1), "<") %>
           [Group
           <% int i = 1;
              foreach (var b in Model.Mission.SubjectGroups)
              {
                if (b.Number == Model.Number) continue;
                i = b.Number;
                Response.Write(Html.ActionLink<MissionsController>(f => f.MoveSubjectToGroup(link.Id, b.Number), b.Number.ToString()) + " ");
              }
              Response.Write(Html.ActionLink<MissionsController>(f => f.MoveSubjectToGroup(link.Id, i+1), "New"));
                 %>]
           <%= Html.ActionLink<MissionsController>(f => f.MoveSubjectOrder(link.Id, 1), ">") %>
           <% } %>
           </div>
           <%
         }
         %>
