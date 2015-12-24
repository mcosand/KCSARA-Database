<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IRosterEntry>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<%
  TrainingRoster row = (TrainingRoster)Model;
  ViewData["courses_" + row.Id.ToString()] = BuildCoursesDropdown((Training)row.GetEvent(), row);
%>
<td>
  <%= Html.CheckBoxList("courses_" + row.Id.ToString(), (IEnumerable<SelectListItem>)ViewData["courses_"+row.Id.ToString()]) %>
</td>

<script language="C#" runat="server">
  protected MultiSelectList BuildCoursesDropdown(Training t, TrainingRoster row)
  {
    Dictionary<string, string> offered = t.OfferedCourses.ToDictionary(f => f.Id.ToString(), f => f.DisplayName);
    string[] awarded = row.TrainingAwards.Select(f => f.Course.Id.ToString()).ToArray();

    return new MultiSelectList(offered, "Key", "Value", awarded);
  }

</script>