<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IRosterEntry>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>

<% MissionRoster row = (MissionRoster)Model; %>
<td>
  <%= Html.DropDownList("unit_" + row.Id.ToString(), BuildUnitDropdown(row)) %>
  <%= Html.ValidationMessage("unit_" + row.Id.ToString()) %>
</td>

<script language="C#" runat="server">
  protected SelectList BuildUnitDropdown(MissionRoster row)
  {
    List<object> list = new List<object>();

    using (var db = new KcsarContext())
    {
        foreach (SarUnit u in (IEnumerable<SarUnit>)ViewData["UnitList"])
        {
            string display = u.DisplayName;

            if (row.Person != null && !db.Members.Single(f => f.Id == row.Person.Id).GetActiveUnits().Select(x => x.Unit.Id).Contains(u.Id))
            {
                display = "! " + display;
            }
            list.Add(new { Id = u.Id, DisplayName = display });
        }
    }
    return new SelectList(list, "Id", "DisplayName", (row.Unit == null) ? Guid.Empty : row.Unit.Id);
  }
</script>