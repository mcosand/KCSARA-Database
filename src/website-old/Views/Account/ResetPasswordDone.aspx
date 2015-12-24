<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="c" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Reset Password</h2>
    <p>
        Your password has been reset, and a new password mailed to: <%= ViewData["Email"] %>.
    </p>
</asp:Content>
