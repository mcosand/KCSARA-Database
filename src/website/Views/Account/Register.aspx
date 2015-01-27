<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

  <h2>Register for a <%= Strings.DatabaseName %> account.</h2>
  <div style="max-width: 50em" id="content">
    <p>
      Welcome to King County Search and Rescue. Current SAR volunteers should use the form below to gain access to the member database. If you are not a current member
      you may be able to apply online using our <%= Html.ActionLink<AccountController>(f => f.Signup(), "online application") %> form.
    </p>
    <p>There are a few prerequisites before you will be able to successfully complete registration:</p>
    <ul>
      <li>You are an active member with one or more KCSARA units.</li>
      <li>A database manager has created a record for you in the database and marked you active with your unit(s).</li>
      <li>Your email address has been associated with your record and is not used by any other member. A verification code will be sent to this address.</li>
    </ul>

    <fieldset>
      <legend>Account Information:</legend>
      <div>
        <label for="email">Email Address:</label>
        <input type="text" style="width: 20em" data-bind="value: Email, watermark: 'user@domain.com'" id="email" />
        <span data-bind="visible: Email.working" class="fa fa-spinner fa-spin"></span>
        <span data-bind="text: Email.Status, style: { color: (Email.Status() == 'Ready') ? 'green' : 'red' }"></span>
      </div>
      <div>
        <label for="username">Username:</label>
        <input type="text" id="username" data-bind="value: Username, valueUpdate: 'afterkeydown'" />
        <span data-bind="text: Username.Available, style: { color: (Username.Available() == 'Available') ? 'green' : 'red' }"></span>
        <label for="password">Password:</label>
        <input type="password" id="password" data-bind="value: Password, valueUpdate: 'afterkeydown'" />
        <span style="color: red" data-bind="text: Password.Errors"></span>
        <br />
        <label for="password2">Verify Password:</label>
        <input type="password" id="password2" data-bind="value: Password.Check, valueUpdate: 'afterkeydown'" />
      </div>
    </fieldset>
    <p>Please review the information above. Once you press the Submit button below an account will be created. You will receive an email with a link to confirm your address and to access the database.</p>
    <button data-bind="jqButtonEnable: Ready() && !Working(), click: function () { doSubmit($root); }">Submit</button>
  </div>
  <script type="text/javascript">
    var PageModel = function () {
      var self = this;

      this.Email = ko.observable('');
      this.Email.watertext = "user@domain.com";
      this.Email.Status = ko.observable();
      this.Email.working = ko.observable(false);
      this.Email.Checker = ko.computed(function () {
        if (this.Email().length > 3) {
          self.Email.working(true);
          $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller="Account", action="CheckEmail" }) %>/' + this.Email(), dataType: 'json', contentType: 'application/json; charset=utf-8' })
          .done(function (result) {
            self.Email.Status(result);
          })
          .always(function () { self.Email.working(false); });
        } else {
          self.Email.Status(null);
        }
      }, this);

      this.Username = ko.observable('');
      this.Username.Error = ko.computed(function () {

      }, this);
      this.Username.Available = ko.observable();
      this.Username.working = ko.observable(false);
      this.Username.Checker = ko.computed(function () {
        if (this.Username().length > 3) {
          self.Username.working(true);
          $.ajax({ type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "CheckUsername" }) %>/' + this.Username(), dataType: 'json', contentType: 'application/json; charset=utf-8' })
                  .done(function (result) {
                    self.Username.Available(result);
                  })
                  .always(function() { self.Username.working(false) } );
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
              var required = ["Email", "Username", "Password"];
              for (var i = 0; i < required.length; i++) {
                var f = required[i];
                ready &= (this[f]() != "" && this[f]() != this[f].watertext);
              }
              return ready;
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
        type: 'POST', url: '<%= Url.RouteUrl("defaultApi", new { httproute="", controller = "Account", action = "Register" }) %>',
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
