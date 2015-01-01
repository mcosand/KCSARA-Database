<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage<AnimalOwner>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Owner Information for <%= Model.Animal.Name %></h2>
    <% using (Html.BeginForm()) { %>
      <%= Html.Hidden("NewOwnerGuid") %>
      <%= Html.Hidden("Animal", Model.Animal.Id) %>
      
      <%= Html.ValidationSummary() %>
   
     <p>
      <label for="name_a">
        Owner:</label>
        <%= Html.ValidationMessage("Owner") %>
      <%= Html.Hidden("pid_a") %>
      <%= Html.TextBox("name_a", (Model.Owner == null) ? "" : Model.Owner.ReverseName, new { style = "display:inline; width:20em;" + ((Model.Owner == null) ? " background-color:#ffffbb" : ""), @class = "input-box" })%>
      <%= Html.TextBox("dem_a", (Model.Owner == null) ? "" : Model.Owner.DEM,
                                                  new { style = "width:4em;" + ((Model.Owner != null) ? "" : " background-color:#ffffbb") })%>
      <%= Html.ValidationMessage("DisplayName", "*") %>
    </p>
    
    <p>
      <label for="IsPrimary">Is Primary Owner?</label>
      <%= Html.CheckBox("IsPrimary") %>
      <%= Html.ValidationMessage("IsPrimary") %>
    </p>
    
    <p>
      <label for="Starting">Starting:</label>
        <%= Html.ValidationMessage("Starting") %>
      <%= Html.TextBox("Starting", Model.Starting.ToString("yyyy-MM-dd"), new { @class = "input-box", style="display:inline;" })%>
    </p>

    <p>
      <label for="Ending">Ending:</label>
        <%= Html.ValidationMessage("Ending") %>
      <%= Html.TextBox("Ending", string.Format("{0:yyyy-MM-dd}", Model.Ending), new { @class = "input-box", style = "display:inline;" })%>
    </p>
    
    <input type="submit" value="Save" class="button button-big" />
  <% } %>
  <script type="text/javascript">
    $(function() {

      $('#name_a').suggest("<%= Html.BuildUrlFromExpression<MembersController>(x => x.Suggest(null)) %>", { dataContainer: "a" });
      
      $('#Starting').datepicker({
        dateFormat: 'yyyy-mm-dd',
        changeMonth: true, changeYear: true,
        showOn: 'button', buttonImage: '<%=this.ResolveUrl("~/Content/images/calendar.gif") %>', buttonImageOnly: true
      });

      $('#Ending').datepicker({
        dateFormat: 'yyyy-mm-dd',
        changeMonth: true, changeYear: true,
        showOn: 'button', buttonImage: '<%=this.ResolveUrl("~/Content/images/calendar.gif") %>', buttonImageOnly: true
      });
    });    
  </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
