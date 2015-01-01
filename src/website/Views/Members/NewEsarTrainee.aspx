<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="System.Web.Mvc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>New ESAR Trainee</h2>
<%= Html.ValidationSummary() %>
<% Html.BeginForm(); %>

<p>
<label for="date">Date of Course A:</label>
<%= Html.TextBox("CourseDate", string.Format("{0:yyyy-MM-dd}", ViewData["CourseDate"])) %><%= Html.ValidationMessage("CourseDate")%>
<script type="text/javascript">applyDTP('CourseDate',false);</script>
</p>


<fieldset title="Member"><legend>Trainee Information</legend>
<p><label for="FirstName">First Name:</label>
<%= Html.TextBox("FirstName") %><%= Html.ValidationMessage("FirstName")%>
</p>

<p><label for="MiddleName">Middle Name:</label>
<%= Html.TextBox("MiddleName") %><%= Html.ValidationMessage("MiddleName")%></p>

<p><label for="LastName">Last Name:</label>
<%= Html.TextBox("LastName") %><%= Html.ValidationMessage("LastName")%></p>

<p><label for="BirthDate">Birth Date:</label>
<%= Html.TextBox("BirthDate") %><%= Html.ValidationMessage("BirthDate")%>
<script type="text/javascript">
  applyDTP('BirthDate', false);
  $('#BirthDate').datepicker('option', 'yearRange', '-100:0');</script>
</p>

<p><label for="SheriffApp">KCSO Application Received:</label>
<%= Html.TextBox("SheriffApp") %><%= Html.ValidationMessage("SheriffApp")%>
<script type="text/javascript">
    applyDTP('SheriffApp', false);
    $('#SheriffApp').datepicker('option', 'yearRange', '-100:0');</script>
</p>


<p><label for="Gender">Gender:</label><%= Html.DropDownList("Gender") %><%= Html.ValidationMessage("Gender")%></p>
</fieldset>

<fieldset><legend>Contact Information</legend>
<p><label for="Street">Street:</label>
<%= Html.TextBox("Street") %><%= Html.ValidationMessage("Street")%></p>
<p><label for="City">City:</label>
<%= Html.TextBox("City") %><%= Html.ValidationMessage("City")%></p>
<p><label for="State">State:</label>
<%= Html.TextBox("State") %><%= Html.ValidationMessage("State")%></p>
<p><label for="Zip">Zip Code:</label>
<%= Html.TextBox("Zip") %><%= Html.ValidationMessage("Zip")%></p>


<p><label for="HomePhone">Home Phone:</label>
<%= Html.TextBox("HomePhone") %><%= Html.ValidationMessage("HomePhone")%></p>
<p><label for="CellPhone">Cell Phone:</label>
<%= Html.TextBox("CellPhone") %><%= Html.ValidationMessage("CellPhone")%></p>
<p><label for="WorkPhone">Work Phone:</label>
<%= Html.TextBox("WorkPhone") %><%= Html.ValidationMessage("WorkPhone")%></p>
<p><label for="Email">Email:</label>
<%= Html.TextBox("Email") %><%= Html.ValidationMessage("Email")%></p>
<p><label for="Email2">Secondary Email:</label>
<%= Html.TextBox("Email2") %><%= Html.ValidationMessage("Email2")%></p>

<p><label for="HamCall">HamCall:</label>
<%= Html.TextBox("HamCall") %><%= Html.ValidationMessage("HamCall")%></p>

</fieldset>
<input type="submit" value="Save" />
<% Html.EndForm(); %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
