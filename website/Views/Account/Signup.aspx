<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Search and Rescue Volunteer Application</h2>
    <div style="max-width: 50em" id="application">
        <p>Welcome to King County Search and Rescue. Use the form below to start the application process with one or more of our <a target="_blank" href="http://kcsara.org/units/">member units</a>.</p>
        <p>Once you have created a basic profile you will continue the application process by adding contact information and other information before applying to a unit.</p>

        <fieldset>
            <legend>Personal Information:</legend>
            <div>
                <label for="firstname">Volunteer Name: <span class="labelExtra">(For background check, please use full legal name.)</span></label>
                <input id="firstname" type="text" style="width: 14em" data-bind="value: Firstname, watermark: Firstname.watertext" />
                <input type="text" style="width: 10em; margin-left: 1em;" data-bind="value: Middlename, watermark: Middlename.watertext" />
                <input type="text" style="width: 16em; margin-left: 1em" data-bind="value: Lastname, watermark: Lastname.watertext" />
            </div>
            <div>
                <label for="BirthDate">Birthdate:</label>
                <input type="text" data-bind="value: Birthdate, watermark: 'yyyy-mm-dd'" id="BirthDate" style="margin-right: .1em; width: 10em" />
            </div>
            <div>
                <label for="gender">Gender:</label>
                <select data-bind="value: Gender" id="gender">
                    <option value="m">Male</option>
                    <option value="f">Female</option>
                    <option>Other/Undisclosed</option>
                </select>
            </div>
            <div>
                <label for="email">Email Address:</label>
                <input type="text" style="width: 20em" data-bind="value: Email, watermark: 'user@domain.com'" id="email" />
            </div>
        </fieldset>
        <fieldset>
            <legend>Select Units:</legend>
            <p>Choose at least one unit for which you would like to volunteer. Questions about the application process may be directed to listed email contact.</p>
            <table>
                <% foreach (var unit in ViewBag.Units) { %>
                  <tr><td><input type="checkbox" <%= unit.IsAcceptingApps ? "" : "disabled=\"disabled\"" %> data-bind="checked: Units" value="<%: unit.Id %>" /></td>
                      <td><%= unit.Name %></td>
                      <td><% if(unit.IsAcceptingApps) { %><a href="mailto:<%= (string)unit.Contact %>"><%= (string)unit.Contact %></a><% } else { %><%: (string)unit.NoAppReason %><% } %></td>
                  </tr>
                <% } %>
            </table>
        </fieldset>
        <fieldset>
            <legend>Login Information:</legend>
            <div>
                <label for="username">Username:</label>
                <input type="text" id="username" data-bind="value: Username, valueUpdate: 'afterkeydown'" />
                <span data-bind="text: Username.Available, style: { color: (Username.Available() == 'Available') ? 'green' : 'red' }"></span>
                <label for="password">Password:</label>
                <input type="password" id="password" data-bind="value: Password, valueUpdate: 'afterkeydown'" />
                <span style="color: red" data-bind="text:Password.Errors"></span>
                <br />
                <label for="password2">Verify Password:</label>
                <input type="password" id="password2" data-bind="value: Password.Check, valueUpdate: 'afterkeydown'" />
            </div>
        </fieldset>
        <label for="todoList">Tasks remaining:</label>
        <ul data-bind="foreach: Ready.Tasks">
            <li data-bind="text: title, visible: !check()"></li>
        </ul>
        <p>Please review the information above. Once you press the Submit button below a profile will be created. You will receive an email with a link to confirm your address and to access your profile.</p>
        <button data-bind="jqButtonEnable: Ready() && !Working(), click: function() { doSubmit($root); }">Submit</button>
    </div>
    <script type="text/javascript">
        var PageModel = function () {
            var self = this;

            this.Firstname = ko.observable('');
            this.Firstname.watertext = "First Name";
            this.Middlename = ko.observable('');
            this.Middlename.watertext = "Full Middle Name";
            this.Lastname = ko.observable('');
            this.Lastname.watertext = "Last Name";

            this.Birthdate = ko.observable('');
            this.Birthdate.watertext = "yyyy-mm-dd";

            this.Gender = ko.observable('m');
            this.Email = ko.observable('');
            this.Email.watertext = "user@domain.com";

            this.Units = ko.observableArray([]);

            this.Username = ko.observable('');
            this.Username.Error = ko.computed(function () {

            }, this);
            this.Username.Available = ko.observable();
            this.Username.Checker = ko.computed(function () {
                if (this.Username().length > 3) {
                    $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "CheckUsername" }) %>/' + this.Username(), dataType: 'json', contentType: 'application/json; charset=utf-8' })
                    .done(function (result) {
                        self.Username.Available(result);
                    });
                }
                else {
                    self.Username.Available(null);
                }
            }, this);


            this.Password = ko.observable('');
            this.Password.Check = ko.observable('');
            this.Password.Errors = ko.computed(function () {
                if (this.Password() != this.Password.Check()) {
                    return "Passwords don't match";
                }
                return '';
            }, this);

            this.Ready = ko.observable();
            this.Ready.Tasks = [
                {
                    title: "Fill in the form", check: ko.computed(function () {
                        var ready = true;
                        var required = ["Firstname", "Lastname", "Birthdate", "Email", "Username", "Password"];
                        for (var i = 0; i < required.length; i++) {
                            var f = required[i];
                            ready &= (this[f]() != "" && this[f]() != this[f].watertext);
                        }
                        return ready;
                    }, this)
                },
                {
                    title: "Select one or more units", check: ko.computed(function() {
                        return this.Units().length > 0;
                    }, this)
                },
                {
                    title: "Choose available username (4 characters or more)", check: ko.computed(function () {
                        return this.Username.Available() == 'Available';
                    }, this)
                },
                {
                    title: "Select password (6 characters or more)", check: ko.computed(function () {
                        return this.Password().length >= 6 && this.Password.Errors() == '';
                    }, this)
                },
            ];

            this.Ready.Checker = ko.computed(function () {
                var ready = true;
                for (var i = 0; i < this.Ready.Tasks.length; i++) {
                    ready &= this.Ready.Tasks[i].check();
                }
                this.Ready(ready);
            }, this);

            this.Working = ko.observable(false);
        };

        function doSubmit(model) {
            model.Working(true);
            $.ajax({
                type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "Signup" }) %>',
                data: ko.toJSON(model), dataType: 'json', contentType: 'application/json; charset=utf-8'
            })
            .done(function (result) {
                if (result == "OK") {
                    window.location.href = '<%=  Url.Action("Verify") %>/' + model.Username();
                }
                else {
                    alert(result);
                }
            })
            .fail(function(result) {
                if (result.responseJSON["data.Birthdate"]) alert('Invalid birthdate');
            })
            .always(function () {
                model.Working(false);
            });
        }



        $(document).ready(function () {
            var a = $('#application');
            a.find('input[type="text"]').addClass('input-box');
            a.find('input[type="password"]').addClass('input-box');
            a.find('select').addClass('input-box');
            var model = new PageModel();
            ko.applyBindings(model);
            $('button').button();

            applyDTP('BirthDate', false);
            $('#BirthDate').datepicker('option', 'yearRange', '-100:-13').datepicker('option', 'defaultDate', '1990-07-01');
        });
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .watermark {
            color: #AAA;
        }

        input[type="text"] {
            margin-right: 1em;
        }

        .input-box {
            display: inline;
        }

        legend {
            margin: 0px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="secondaryNav" runat="server">
</asp:Content>
