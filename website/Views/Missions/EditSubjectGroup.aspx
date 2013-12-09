<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" AutoEventWireup="true"
  Inherits="System.Web.Mvc.ViewPage<SubjectGroup>" %>

<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
<style type="text/css">.fields label { font-weight:normal; }</style>
<% 
  object inputBox = new { @class = "input-box", style = "display:inline" };
   object dateBox = new { @class = "input-box", style="display:inline" };
  Dictionary<string,object> checkboxAttribs = new Dictionary<string,object>();
  checkboxAttribs.Add("display", "table-cell");
   %>
<% Html.BeginForm(); %>
<div style="clear:both; margin-top:.5em;">
<table>
 <tr><th colspan="2">Time Line</th></tr>
 <tr><td>Lost:</td><td><span style=""><%= Html.DateTimePickerFor(f => f.WhenLost, dateBox) + Html.ValidationMessage("WhenLost") %> </span></td></tr>
 <tr><td>Reported:</td><td><%= Html.DateTimePickerFor(f => f.WhenReported, dateBox) + Html.ValidationMessage("WhenReported") %></td></tr>
 <tr><td>Called SAR:</td><td><%= Html.DateTimePickerFor(f => f.WhenCalled, dateBox) + Html.ValidationMessage("WhenCalled") %></td></tr>
 <tr><td>Searchers At PLS:</td><td><%= Html.DateTimePickerFor(f => f.WhenAtPls, dateBox) + Html.ValidationMessage("WhenAtPls") %></td></tr>
 <tr><td>Found:</td><td><%= Html.DateTimePickerFor(f => f.WhenFound, dateBox) + Html.ValidationMessage("WhenFound") %></td></tr>
 <tr><th colspan="2">Locations</th></tr>
<tr><td>
  PLS Latitude:</td><td>
  <%= Html.EditorFor(f => f.PlsNorthing, inputBox)%>
</td></tr>
<tr><td>
  PLS Longitude:</td><td>
  <%= Html.EditorFor(f => f.PlsEasting, inputBox)%>
</td></tr>
<tr><td>
  PLS Common Name:</td><td>
  <%= Html.EditorFor(f => f.PlsCommonName, inputBox)%>
</td></tr>
<tr><td>
  Found, Latitude:</td><td>
  <%= Html.EditorFor(f => f.FoundNorthing, inputBox)%>
</td></tr>
<tr><td>
  Found, Longitude:</td><td>
  <%= Html.EditorFor(f => f.FoundEasting, inputBox)%>
</td></tr>
</table>
<table>
<tr><th>Category(s)</th><th>Cause(s)</th><th>Behavior(s)</th><th>Found By</th><th>Condition</th></tr>
<tr><td>
  <%= Html.CheckBoxList("CategoryList", (MultiSelectList)ViewData["CategoryList"]) %>
    <div class="fields"><input type="checkbox" id="categoryFoo" /> <%= Html.TextBox("CategoryOther", ViewData["CategoryOther"], inputBox) %></div>
</td>
<td>
  <%= Html.CheckBoxList("CauseList", (MultiSelectList)ViewData["CauseList"]) %>
    <div class="fields"><input type="checkbox" id="Checkbox1" /> <%= Html.TextBox("CauseOther", ViewData["CauseOther"], inputBox) %></div>
</td>
<td>
  <%= Html.CheckBoxList("BehaviorList", (MultiSelectList)ViewData["BehaviorList"])%>
    <div class="fields"><input type="checkbox" id="Checkbox2" /> <%= Html.TextBox("BehaviorOther", ViewData["BehaviorOther"], inputBox)%></div>
</td>
<td>
  <%= Html.CheckBoxList("FoundList", (MultiSelectList)ViewData["FoundList"])%>
    <div class="fields"><input type="checkbox" id="Checkbox3" /> <%= Html.TextBox("FoundOther", ViewData["FoundOther"], inputBox)%></div>
</td>
<td>
  <%= Html.CheckBoxList("ConditionList", (MultiSelectList)ViewData["ConditionList"])%>
</td>
</tr>
</table>
<label for="Comments">Comments:</label>
<%= Html.EditorFor(f => f.Comments)%>
</div>
<input type="submit" value="Save" />
<% Html.EndForm(); %>

</asp:Content>
