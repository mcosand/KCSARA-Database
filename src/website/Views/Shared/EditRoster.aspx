<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ExpandedRowsContext>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
<%@ Import Namespace="Kcsara.Database.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<%
  string controllerPath = (Model.Type == RosterType.Mission) ? "Missions" : "Training";
  List<string> suggestBoxIds = new List<string>();

  Html.RenderPartial(Model.Type.ToString(), Model.SarEvent);
  %>

<h2><%= Model.Type %> Roster</h2>
<% Html.BeginForm("EditRoster", controllerPath); %>
<%= Html.Hidden("DayCount", Model.NumDays) %>
<%= Html.Hidden("StartDay", Model.RosterStart) %>

<input type="submit" name="button" value="<%= Strings.AddMore %>" /> <input type="submit" name="button" value="<%= Strings.FinishRoster %>" />
<table id="roster" cellpadding="0" class="data-table">
  <thead>
    <tr>
      <th rowspan="2">Delete?</th>
      <th rowspan="2">Name</th>
      <th rowspan="2">DEM</th>
      <% if (Model.Type == RosterType.Mission)
         { %>
      <th rowspan="2">Unit</th>
      <th rowspan="2">Role</th>
      <% }
         else
         { %>
      <th rowspan="2">Completed Training</th>
      <% } %>
      <% for (int i = 0; i < Model.NumDays; i++) { %>
        <th colspan="2">
          <% if (i == 0) Response.Write(Html.SubmitImage("AddDayLeft", this.ResolveUrl("~/Content/images/roster-add-left.png"))); %>
          <%= Model.RosterStart.AddDays(i).ToShortDateString()%>
          <% if (i == Model.NumDays - 1) Response.Write(Html.SubmitImage("AddDayRight", this.ResolveUrl("~/Content/images/roster-add-right.png"))); %>
        </th>
      <% } %>
      <th rowspan="2">Hours</th>
      <th rowspan="2">Miles</th>
    </tr>
    <tr>
      <% for (int i = 0; i < Model.NumDays; i++) { %>
        <th style="font-size:85%">Time In</th>
        <th style="font-size:85%">Time Out</th>
      <% } %>
    </tr>
  </thead>
  <tbody>
    <%
      IEnumerable<IRosterEntry> rows = Model.Rows;
    
    foreach (IRosterEntry row in rows)
    {      
      IRosterEvent evt = row.GetEvent();
      suggestBoxIds.Add(row.Id.ToString());

      %>
    <tr>
        <td>
           <%= Html.Hidden("id_" + row.Id.ToString(), row.Id) %>
           <%= Html.Hidden("pid_" + row.Id.ToString(), (row.Person == null) ? Guid.Empty : row.Person.Id) %>
           <%= Html.CheckBox("del_" + row.Id.ToString(), false, new { onClick = "return confirm('Mark for deletion?');" }) %>
        </td>
        <td><%= Html.TextBox("name_" + row.Id.ToString(),
                              (row.Person == null) ? "" : row.Person.ReverseName,
                              (row.Person == null) ? (object)new { style="background-color:#ffffbb" } : (object)new { @class = "input-box" }).ToString()
              %><%= Html.ValidationMessage("name_" + row.Id.ToString(), new { style = "display:block;" }) %>
              <% if (row is MissionRoster) {
                   bool showDiv = false;
                   MultiSelectList animalList = BuildAnimalDropdown((MissionRoster)row, out showDiv);
                   string display = showDiv ? "block" : "none";
                   %>
                <div id="animalDiv_<%= row.Id %>" style="padding-left:1.5em; display: <%= display %>;">
                  <label for="animal_<%= row.Id %>">With animal(s):</label>
                  <%= Html.ListBox("animal_" + row.Id.ToString(), animalList, new { @class = "input-box" }) %>
                  <%= Html.ValidationMessage("animal_" + row.Id.ToString(), new { style = "display:block;" }) %>
                </div>
              <% } %>
        </td>

        <td><%= Html.TextBox("dem_" + row.Id.ToString(),
                              (row.Person == null) ? "" : row.Person.DEM,
                                            new { style = "width:4em;" + ((row.Person != null) ? "" : " background-color:#ffffbb"), @class = "input-box" }).ToString()
              + Html.ValidationMessage("dem_" + row.Id.ToString(), new { style = "display:block;" }) %></td>
              
        <% 
        string otDisplay = "none";
        if (row is MissionRoster)
         {
             MissionRoster mrow = (MissionRoster)row;
             bool showOtDiv = false;
             string unitOptions = BuildUnitDropdownOptions((MissionRoster)row, out showOtDiv);
             otDisplay = showOtDiv ? "block" : "none";
        %>
        <td>
          <select name="unit_<%= row.Id.ToString() %>" id="unit_<%= row.Id.ToString() %>" onchange="UnitChanged(this)" class="input-box" >
            <%= unitOptions %>
          </select>
          <%: Html.ValidationMessage("unit_" + row.Id.ToString()) %>
        </td>
        <td>

        
        
        
        <%: Html.DropDownList("role_" + row.Id.ToString(), new SelectList((string[])ViewData["RoleTypes"], mrow.InternalRole), new { @class = "input-box" })%>
        
        
        <%: Html.ValidationMessage("role_"+row.Id.ToString()) %></td>
        <% }
           else if (row is TrainingRoster)
           {
             TrainingRoster trow = (TrainingRoster)row;
             Response.Write("<td>");
             ViewData["courses_" + row.Id.ToString()] = BuildCoursesDropdown((Training)Model.SarEvent, trow);
             Response.Write(Html.ValidationMessage("courses_" + row.Id.ToString(), new { style = "display:block;" }));
             Response.Write(Html.CheckBoxList("courses_" + row.Id.ToString(), (MultiSelectList)ViewData["courses_" + row.Id.ToString()]));
             Response.Write("</td>");
           }
           else
           { %>
          <td></td>
        <% } %>

        <% for (int i = 0; i < Model.NumDays; i++) { %>
          <td><%= TimeInOutBox("in", Model.RosterStart.AddDays(i), row.Id, row.TimeIn) %></td>
          <td><%= TimeInOutBox("out", Model.RosterStart.AddDays(i), row.Id, row.TimeOut) %></td>
        <% } %>

        <td class="r">
        <% if (row is MissionRoster)
           { %>
           <div id="otd_<%= row.Id.ToString() %>" style="display:<%= otDisplay %>">
            <label for="overtimehours_<%= row.Id.ToString() %>">OT Hours:</label>
           <%= Html.ValidationMessage("overtimehours_" + row.Id.ToString(), new { style = "display:block;" }) %>
           <%= Html.TextBox("overtimehours_" + row.Id.ToString(), ((MissionRoster)row).OvertimeHours, new { style = "width:3em;", @class = "input-box" }) %>
        <% } %>
          </div>
          <%= (row.TimeIn.HasValue && row.TimeOut.HasValue) ? (row.TimeOut.Value - row.TimeIn.Value).TotalHours.ToString("0.0") : ""%>
        </td>
        <td class="r"><%= Html.TextBox("miles_" + row.Id.ToString(), row.Miles, new { style = "width:2em;", @class = "input-box" })%></td>
      </tr>
  <% }%>
  </tbody>
</table>
<input type="submit" name="button" value="<%= Strings.AddMore %>" /> <input type="submit" name="button" value="<%= Strings.FinishRoster %>" />
<% Html.EndForm(); %>
<%= Html.Image(this.ResolveUrl("~/Content/images/progress-sm.gif"), new { id="progress", style="position:absolute; visibility:hidden;" }) %>

<script type="text/javascript">
var suggestBoxes = new Array('<%= string.Join("','", suggestBoxIds.ToArray()) %>');

$(document).ready(function () {
  for (var i = 0; i < suggestBoxes.length; i++)
  {
    $("#name_" + suggestBoxes[i]).suggest(
      "<%= Html.BuildUrlFromExpression<MembersController>(x => x.Suggest(null)) %>",
      { dataContainer: suggestBoxes[i], onSelect: function(id) { OnSelected(id) } });
  }
});

function UnitChanged(sender)
{
  var otBox = jQuery("#"+sender.id.replace("unit_", "overtimehours_"));
  var otDiv = jQuery("#"+sender.id.replace("unit_", "otd_"));

  var hasOt = (sender.options[sender.selectedIndex].attributes["hasOt"] != undefined);
  otDiv.css({ display: hasOt ? "block" : "none" });
  if (! hasOt)
  {
   otBox.val('');
  }
}

function OnSelected(id) {
<% if (Model.Type == RosterType.Mission) { %>  
  var list = jQuery("#unit_"+id);
  list.disabled = true;
  var oldColor = list.css("backgroundColor");
  list.css('backgroundColor', "Orange");
 // $("#statusprogress").css({visibility:"visible"});
  var url = '<%= Url.Action("GetMembersActiveUnits", "Members") %>' + '/' + jQuery("#pid_"+id).val()+'?when=<%= Model.RosterStart.ToString("yyyy-MM-dd") %>';

  jQuery.getJSON(url, null, function(data) {
    var result = eval(data);
    var options = '';
    
    var lookup = new Object();
    for (var i=0; i < list[0].options.length; i++)
    {
      if (list[0].options[i].text[0] != '!')
      {
        list[0].options[i].text = '! ' + list[0].options[i].text;
      }
      lookup[list[0].options[i].value.toLowerCase()] = list[0].options[i];
    }
    
    var selected = false;
    for (var i = 0; i < result.length; i++) {
      var opt = lookup[result[i].Id.toLowerCase()];
      opt.text = opt.text.substring(2);
      if (!selected)
      {
        list[0].selectedIndex = opt.index;
        selected = true;
      }
    }
  //  jQuery("#statusprogress").css({ visibility: "hidden" });
    list.css('backgroundColor', oldColor);
    list.disabled = false;
    UnitChanged(list[0]);
  });
  
  // Update list of animals
  var animalList = jQuery("#animal_"+id);
  var animalDiv = jQuery("#animalDiv_"+id);
  animalList.disabled = true;
  var oldAnimalColor = animalList.css("backgroundColor");
  animalList.css('backgroundColor', "Orange");
  
  jQuery.getJSON('<%= Url.Action("GetMembersAnimals", "Members") %>' + '/' + jQuery("#pid_"+id).val()+'?when=<%= Model.RosterStart.ToString("yyyy-MM-dd") %>', null, function(animalData) {
    var animalResult = eval(animalData);
    var optionsHtml = "";

    if (animalResult.length == 0)
    {
        animalDiv.css({ display: "none" });
    }
    else
    {
      for (var i = 0; i < animalResult.length; i++) {
        optionsHtml += '<option value="' + animalResult[i].Id + '">[' + animalResult[i].Type + '] ' + animalResult[i].Name + '</option>';
      }
      animalDiv.css({ display: "block" });
    }

    animalList.html(optionsHtml);
    animalList.css('backgroundColor', oldAnimalColor);
    animalList.disabled = false;
    
  });
<% } %>  
  }

</script>


</asp:Content>

<script language="C#" runat="server">
    KcsarContext db = new KcsarContext();
    public override void Dispose()
    {
        db.Dispose();
        base.Dispose();        
    }
    
  protected MultiSelectList BuildAnimalDropdown(MissionRoster row, out bool showDiv)
  {
    List<object> list = new List<object>();
    int count = 0;
      
    if (row.Person != null)
    {        
      foreach (Animal a in db.Members.Single(f => f.Id == row.Person.Id).Animals.Select(f => f.Animal))
      {
        list.Add(new { Id = a.Id, DisplayName = string.Format("[{0}] {1}", a.Type, a.Name) });
        count++;
      }
    }
    showDiv = (row.Person != null) && (count > 0);
    return new MultiSelectList(list, "Id", "DisplayName", db.AnimalMissions.Where(f => f.MissionRoster.Id == row.Id).Select(f => f.Animal.Id));
  }
  
  protected SelectList BuildUnitDropdown(MissionRoster row)
  {
    List<object> list = new List<object>();


    foreach (SarUnit u in (IEnumerable<SarUnit>)ViewData["UnitList"])
    {
      string display = u.DisplayName;

      if (row.Person != null && !row.Person.GetActiveUnits().Select(x => x.Unit.Id).Contains(u.Id))
      {
        display = "! " + display;
      }
      list.Add(new { Id = u.Id, DisplayName = display, Overtime = u.HasOvertime });
    }

    return new SelectList(list, "Id", "DisplayName", (row.Unit == null) ? Guid.Empty : row.Unit.Id);
  }

  protected string BuildUnitDropdownOptions(MissionRoster row, out bool showOtDiv)
  {
    StringBuilder builder = new StringBuilder();
    showOtDiv = false;

    foreach (SarUnit u in (IEnumerable<SarUnit>)ViewData["UnitList"])
    {
        string display = u.DisplayName;


        if (row.Person != null)
        {
            var units = db.Members.Single(f => f.Id == row.Person.Id).GetActiveUnits().Select(f => f.Unit.Id);
            if (!units.Contains(u.Id))
            {
                display = "! " + display;
            }
        }
        builder.AppendFormat("<option value=\"{0}\"{1}{2}>{3}</option>",
            u.Id,
            (row.Unit != null && row.Unit.Id == u.Id) ? "selected=\"selected\"" : "",
            u.HasOvertime ? "HasOt=\"true\"" : "",
            display);
        showOtDiv |= ((row.Unit != null && row.Unit.Id == u.Id) && u.HasOvertime);
    }
    return builder.ToString();
  }
  
  
  protected MultiSelectList BuildCoursesDropdown(Training t, TrainingRoster row)
  {
    Dictionary<string, string> offered = t.OfferedCourses.ToDictionary(f => f.Id.ToString(), f => f.DisplayName);
    string[] awarded = row.TrainingAwards.Select(f => f.Course.Id.ToString()).ToArray();

    return new MultiSelectList(offered, "Key", "Value", awarded);
  }

  protected string TimeInOutBox(string prefix, DateTime date, Guid rowId, DateTime? modelValue)
  {
    string value =
        (modelValue.HasValue && (modelValue.Value.Date == date))
        ? modelValue.Value.ToString("HHmm")
        : "";

    string name = string.Format("{0}{1}_{2}", prefix, date.ToString("yyMMdd"), rowId);
    return string.Format("{0}{1}", Html.TextBox(name, value, new { style = "width:4em;", @class = "input-box" }), 
      Html.ValidationMessage(name));
  }
  </script>