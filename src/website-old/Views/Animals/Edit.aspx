<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage<Animal>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Animal Details</h2>
    <% using (Html.BeginForm()) { %>
      <%= Html.Hidden("NewAnimalGuid") %>
      
      <%= Html.ValidationSummary() %>
   
    <p>
      <label for="Name">Name</label>
      <%= Html.EditorFor(f => f.Name) %>
      <%= Html.ValidationMessage("Name") %>
    </p>
  
    <p>
      <label for="DemSuffix">DEM Suffix</label>
      <%= Html.EditorFor(f => f.DemSuffix) %>
      <%= Html.ValidationMessage("DemSuffix") %>
    </p>
  
    <p>
      <label for="Type">Type</label>
      <%= Html.DropDownList("Type", (SelectList)ViewData["TypeList"]) %>
      <%= Html.ValidationMessage("Type") %>
    </p>

    <p>
      <label for="Comments">Comments</label>
      <%= Html.EditorFor(f => f.Comments) %>
      <%= Html.ValidationMessage("Comments") %>
    </p>
    
    <input type="submit" value="<%= Strings.ActionSave %>" class="button button-big" />
  <% } %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
