<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<IEnumerable<SarUnit>>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
<script type="text/javascript">
  var selected = 0;
  
  function setAllHandler(target, val) {
    var table = target;
    while (table.tagName.toLowerCase() != "table") {
      table = table.parentNode;
    }

    var rows = table.tBodies[0].rows;
    for (var i = 0; i < rows.length; i++) {
      // Assumes select checkbox is the first input in the first cell of the row.
      var input = rows[i].getElementsByTagName("input")[0];
      input.checked = val;
    }
    
    selected = val ? rows.length : 0;
    
    updateToolbarButtons(selected);
  }

  function changeSelectedHandler(target) {
    selected += target.checked ? 1 : -1;
    updateToolbarButtons(selected);
  }

  function updateToolbarButtons(count) {
    var toolbar = document.getElementById("toolbar");
    var buttons = toolbar.getElementsByTagName("input");
    for (var i = 0; i < buttons.length; i++) {
      if (buttons[i].className.indexOf("l-single") >= 0) {
        buttons[i].disabled = (count != 1);
      }
      else if (buttons[i].className.indexOf("l-multi") >= 0) {
        buttons[i].disabled = (count == 0);
      }
    }
  }

</script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <table id="the-table">
    <thead>
      <tr>
<%--        <th style="font-size:6pt">          
          <a id="slct-all" href="#" onclick="setAllHandler(this, true)">All</a><br />
          <a id="slct-none" href="#" onclick="setAllHandler(this, false)">None</a>
        </th>
        <th>DEM</th> --%>
        <th>
          Name
        </th>
      </tr>
    </thead>
    <tbody>
      <% foreach (SarUnit u in ViewData.Model)
         {%>
        <tr>
       <%--   <td><input type="checkbox" name="id" value="<%= u.Id %>" onchange="changeSelectedHandler(this)" /></td> --%>
          <td><%= Html.ActionLink<UnitsController>(x => x.Detail(u.Id), u.DisplayName) %></td>
         </tr>
      <% } %>
    </tbody>
  </table>

<%--<script type="text/javascript">
  var rows = document.getElementById("the-table").tBodies[0].rows;
  for (var i = 0; i < rows.length; i++) {
    if (rows[i].cells[0].getElementsByTagName("input")[0].checked) {
      selected++;
    }
  }
</script>--%>
</asp:Content>
