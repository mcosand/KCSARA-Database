<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IRosterEntry>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<%
    MissionRoster row = (MissionRoster)Model;
  
    bool showDiv = false;
    MultiSelectList animalList = BuildAnimalDropdown((MissionRoster)row, out showDiv);
    string display = showDiv ? "block" : "none";
%>
<div id="animalDiv_<%= row.Id %>" style="padding-left:1.5em; display: <%= display %>;">
  <label for="animal_<%= row.Id %>">With animal(s):</label>
  <%= Html.ListBox("animal_" + row.Id.ToString(), animalList) %>
  <%= Html.ValidationMessage("animal_" + row.Id.ToString(), new { style = "display:block;" }) %>
</div>


<script language="C#" runat="server">
  protected MultiSelectList BuildAnimalDropdown(MissionRoster row, out bool showDiv)
  {
    List<object> list = new List<object>();

    if (row.Person != null)
    {
      foreach (Animal a in row.Person.Animals.Select(f => f.Animal))
      {
        list.Add(new { Id = a.Id, DisplayName = string.Format("[{0}] {1}", a.Type, a.Name) });
      }
    }
    showDiv = (row.Person != null) && (row.Person.Animals.Count > 0);
    return new MultiSelectList(list, "Id", "DisplayName", row.Animals.Select(f => f.Animal.Id));
  }
</script>