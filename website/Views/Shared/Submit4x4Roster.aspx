<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<Kcsar.Database.Model.RuleViolationsException>" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>
    <%
        ExpandedRowsContext data = (ExpandedRowsContext)ViewData["data"];
        foreach (var item in Model.Errors) {
            var modelRow = data.Rows.SingleOrDefault(f => f.Id == item.EntityKey);
     %>
"<%: (modelRow == null) ? "" : modelRow.Person.FullName %>","<%: item.PropertyName %>","<%: item.PropertyValue %>","<%: item.ErrorMessage %>
    <% } %>