<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<h2>Verify KCSARA Account</h2>

    <p>Welcome back! Please make sure the values below match the verification email you received, and then press the Submit button.</p>
    <div id="verify">
    <label for="username">Username:</label>
    <div data-bind="text: Username"></div>
    <label for="key">Verification Key:</label>
    <input type="text" data-bind="value: Key" style="width:30em"/>

    <button data-bind="click: doSubmit">Submit</button>
    </div>

    <script type="text/javascript">
        var PageModel = function () {
            var self = this;
            this.Username = ko.observable(<%= Json.Encode(ViewBag.Username) %>);
            this.Key = ko.observable(<%= Json.Encode(ViewBag.Key) %>);

            this.Working = ko.observable(false);

            this.doSubmit = function () {
                self.Working(true);
                $.ajax({
                    type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "Verify" }) %>',
                    data: ko.toJSON(self), dataType: 'json', contentType: 'application/json; charset=utf-8'
                })
                .done(function (result) {
                    if (result) {
                        alert('Thank you. You are now able to log into the KCSARA database.');
                        window.location.href = '<%=  Url.Action("Login", "Account") %>';
                    }
                    else {
                        alert('Could not verify account');
                    }
                })
                .fail(function() {
                    alert('Error verifying account');
                })
                .always(function () {
                    self.Working(false);
                });
            }
        }

        $(document).ready(function () {
            var a = $('#verify');
            a.find('input[type="text"]').addClass('input-box');
            var model = new PageModel();
            ko.applyBindings(model);
            $('button').button();
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
