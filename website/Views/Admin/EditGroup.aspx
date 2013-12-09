<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Popup.Master" Inherits="System.Web.Mvc.ViewPage<Kcsar.Membership.ExtendedRole>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Edit Group '<%: Model.Name %>'</h2>

    <% using (Html.BeginForm()) {%>
        <%: Html.ValidationSummary(true) %>
        
        <fieldset>
            <legend>Fields</legend>

            <div class="editor-label">
                <%: Html.LabelFor(model => model.Name) %>
            </div>
            <div class="editor-field">
              <%: Html.EditorFor(f => f.Name) %>
              <%: Html.ValidationMessageFor(f => f.Name) %>
            </div>
            
            <div class="editor-label">
                <%: Html.LabelFor(model => model.EmailAddress) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(model => model.EmailAddress) %>
                <%: Html.ValidationMessageFor(model => model.EmailAddress) %>
            </div>

            <div class="editor-label">
                <%: Html.LabelFor(model => model.Destinations) %>
            </div>
            <div class="editor-field">
              <%: Html.TextBox("Destinations") %>
            </div>

            <div class="editor-label">
                <%: Html.LabelFor(model => model.Owners) %>
            </div>
            <div class="editor-field">
              <%: Html.TextBox("Owners") %>
            </div>
            
            <p>
                <input type="submit" value="Save" />
            </p>
        </fieldset>

    <% } %>

    <div>
        <%: Html.ActionLink("Back to List", "Groups") %>
    </div>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

