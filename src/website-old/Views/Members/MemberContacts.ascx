<%@ Control Language="C#" AutoEventWireup="true" Inherits="ViewUserControl<Member>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<% bool canEdit = ViewData["CanEditSelf"] != null && (bool)ViewData["CanEditSelf"]; %>
<div>
  <table class="data-table" id="contacts_table">
  <thead style="display:none;"><tr><th>Type</th><th>SubType</th><th>Value</th><th></th><%= canEdit ? "<th></th>" : "" %></tr></thead>
  <tbody>
  <tr><td colspan="10" style="text-align:center;"><%= Strings.Loading %></td></tr>
  </tbody>
  </table>
    <% if (canEdit) { %><button id="addContact">Add Contact Information</button><% } %>
</div>
<div id="contactform" title="Update Contact Information" style="display:none;">
    <p class="validateTips"></p>

    <form action="#">
    <fieldset>
        <label for="contactType">Type</label>
        <select id="contactType" name="type" class="ui-corner-all input-box" style="display:inline-block" onchange="GetSubtype($('#contactType').val())">
        </select>

        <select id="contactSubtype" name="contactSubtype" class="ui-corner-all input-box" style="display:inline-block" disabled="disabled"></select>

        <label for="contactValue" id="contactValueLabel">Number</label>
        <input type="text" name="contactValue" id="contactValue" class="text ui-corner-all input-box" />
    </fieldset>
    </form>
</div>

<script type="text/javascript">
function GetSubtype(type, selected) {
  $("#contactType").attr('disabled', 'disabled');
  var subType = $("#contactSubType");
  $("#progressbar").offset(subType.offset());
  $("#progressbar").css({visibility:"visible" });
  
  $.ajax({ type: 'POST', url: '<%= Url.Action("GetContactInfoSubTypes") %>', data: { type: type }, dataType: 'json',
    success: function (data) {
      var select = document.getElementById('contactSubtype');
      select.options.length = 0;

      for (option in data.SubTypes)
      {
        var o = new Option(data.SubTypes[option]);
        if (selected != undefined) o.selected = (data.SubTypes[option] == selected);
        select.options[select.options.length] = o;
      }
      
      $("#contactValueLabel").text(data["ValueLabel"]);

      $("#progressbar").css({ visibility: "hidden" });
      $("#contactType").removeAttr('disabled');
      $("#contactSubtype").removeAttr('disabled');
    },
    error: function (request, status, error) {
      $("#progressbar").css({ visibility: "hidden" });
      $("#contactType").removeAttr('disabled');
      $("#contactSubtype").removeAttr('disabled');

      $('#contactform').dialog('close');
      handleDataActionError(request, status, error);
    }
  });

  return true;
}

var contactForm;
$(document).ready(function () {
  var types = ['<%= string.Join("', '", PersonContact.AllowedTypes)  %>'];
  for (var t in types) {
    $("#contactType").append("<option>" + types[t] + "</option>");
  }

  contactForm = new ModelTable("#contacts_table", "#contactform", true);
  contactForm.height = 250;
  <%= canEdit ? "contactForm.canEdit = true;" : "" %>
  contactForm.getUrl = '<%= Url.Action("GetContacts", new { id = Model.Id }) %>';
  contactForm.deleteUrl = '<%= Url.Action("DeleteContact") %>';
  contactForm.postUrl = '<%= Url.Action("SubmitContact") %>';
  contactForm.defaultSort = [[0,0], [3,0], [1,0]];
  contactForm.objectType = "Contact";
  contactForm.renderer = function (data, row) {
        $(row).html('<td style="font-weight:bold;">' + data.Type +
          '</td><td>' + ((data.SubType == null) ? "" : data.SubType) +
          '</td><td>' + data.Value +
          '</td><td>' + ((data.Priority == 0) ? 'Primary' : <%= canEdit ? "('<a href=\"#\" onclick=\"MakePrimary(\\'' + data.Id + '\\'); return false\">Promote</a>')" : "'Secondary'"  %>) +
          '</td>'
          )
         };
  contactForm.formParser = function(c) {
       c.Type = $("#contactType").val();
       c.SubType = $("#contactSubtype").val();
       c.Value = $("#contactValue").val();
       c.MemberId = '<%= Model.Id %>';
       return c;
      };
  contactForm.fillForm = function(item) {
        $('#contactType').val(item.Type);
        if (!GetSubtype(item.Type, item.SubType))
        {
          return false;
        }
        $('#contactValue').val(item.Value);
        return true;
      };
  contactForm.onDelete = function() { contactForm.ReloadData(); }
  contactForm.Initialize();

  $('#addContact').click(function () {
    $("#contactValue").val("");
    contactForm.current = {};
    $('#contactform').dialog('open');
    $('#contactType').focus();
    $('#contactform').dialog('option', 'position', 'center');
  });
});

  function MakePrimary(data)
  {
          $.ajax({ type: 'POST', url: '<%= Url.Action("PromoteContact") %>', data: { id: data }, dataType: 'json',
          success: function (data) {
            if (data.Errors.length == 0) {
              contactForm.ReloadData();
            }
          },
          error: handleDataActionError,
          complete: function (request, status) {
            $('.ui-dialog-buttonpane button').removeAttr('disabled').removeClass('ui-state-disabled');
          }
        });

  }
</script>