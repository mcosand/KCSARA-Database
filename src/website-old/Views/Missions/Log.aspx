<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Missions/Missions.Master" Inherits="System.Web.Mvc.ViewPage<Mission_Old>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<% bool canEdit = Page.User.IsInRole("cdb.missioneditors"); %>
    <% Html.RenderPartial("Mission"); %>

    <h2>Mission Summary</h2>
    <div><%: (Model.Details == null) ? "" : Model.Details.Comments %></div>
    <% if (canEdit) Response.Write(Html.PopupActionLink<MissionsController>(f => f.EditSummary(Model.Id), "Edit", 740, 460)); %>

<%--    <h2>Equipment Lost or Damaged</h2>
    <div><pre><%= Html.Encode((Model.Details == null) ? "" : Model.Details.EquipmentNotes) %></pre></div>
    <% if (canEdit) Response.Write(Html.PopupActionLink<MissionsController>(f => f.EditExpenses(Model.Id), "Edit", 740, 460)); %>--%>
    
    <h2>Mission Log</h2>
    <%= Html.ActionLink("Download ICS-109", "ICS109", new { id = Model.Id }, null)%>
  <table id="missionlog" border="0" cellpadding="0" class="data-table">
  <thead>
  <tr><th>Time</th><th>Entry</th><th>User</th>
  <% if (canEdit) Response.Write("<th></th>"); %>
  </tr>
  </thead>
  <tbody>
  <% foreach (MissionLog log in Model.Log.OrderBy(f => f.Time)) { %>
    <tr>
      <td><%= log.Time.ToEventTime(Model.StartTime) %></td><td><%= log.Data %></td><td><%= (log.Person == null) ? "" : log.Person.ReverseName %></td>
      <% if (canEdit)
         { %>
      <td>
      <%= Html.PopupActionLink<MissionsController>(f => f.EditLog(log.Id), "Edit", 450) %>
      <%= Html.PopupActionLink<MissionsController>(f => f.DeleteLog(log.Id), "Delete") %>
      </td>
      <% } %>
    </tr>
  <% } %>

  </tbody>
</table>
<% if (canEdit) {
 // Response.Write(Html.ActionLink<MissionsController>(x => x.CreateLog(Model.Id), "Add Log Entry", 400));
     %>
     <button id="addlog">Add Log Entry</button>     
     <%
 } %>

<div id="logform" title="New Log Entry">
    <p class="validateTips"></p>

    <form action="#">
    <fieldset>
        <label for="time">Date/Time</label>
        <input type="text" name="time" id="time" class="text ui-corner-all" />
        <label for="data">Entry</label>
        <textarea id="data" name="data" class="ui-corner-all" cols="20" rows="2" style="width:30em; height:10em"></textarea>        
        <label for="person">Logged By</label>
        <input type="text" name="person" id="person" value="" class="text suggest ui-corner-all" />
    </fieldset>
    </form>
</div>
<script type="text/javascript">
  var href = window.location.href;
  var missionDate = new Date('<%= Model.StartTime.Date %>');
  
  function toEventTime(time)
  {
    // Calculate the difference in milliseconds
    var difference_ms = Date.parse(new Date(time).toDateString()) - missionDate.getTime();

    // Convert back to days and return
    var days = Math.floor(difference_ms/(1000 * 60 * 60 * 24));
    return ((days == 0) ? '' : (days + '+')) + formatDateTime("HHii", time);
  }

  $(document).ready(function() {
    $("#missionlog").tablesorter({ widgets: ['zebra'] });
    $('.suggest').suggest2("<%= Html.BuildUrlFromExpression<MembersController>(x => x.Suggest(null)) %>", { dataContainer: "a" });

    var time = $("#time"),
            entry = $("#data"),
            member = $("#person"),
            volatileFields = $([]).add(entry),
            allFields = $([]).add(time).add(entry).add(member),
            tips = $(".validateTips"),
            missionId = '<%= Model.Id %>';

    $("#logform").dialog({
      autoOpen: false,
      height: 400,
      width: 450,
      modal: true,
      buttons: {
        'Log Entry': function() {
          $('.ui-dialog-buttonpane button').attr('disabled', 'true').addClass('ui-state-disabled');
          
          allFields.removeClass('ui-state-error');
          tips.removeClass('ui-state-error');
          $('label > span').remove();

          var d = $(this);
          var s = { Message: entry.val(), Time: time.val(), MissionId: missionId };
          if (member.attr('result')) {
            var p = JSON.parse(member.attr('result'));
            s['Person.Id'] = p.Id;
            s['Person.Name'] = p.FullName;
          }

          var shortRef = href.substr(0, href.toLowerCase().indexOf('/log/'));

          $.ajax({ type: 'POST', url: shortRef + '/submitlog', data: s, dataType: 'json',
            success: function(data) {
              if (data.Errors.length == 0) {
                data.Result.Time = new Date(parseInt(/\/Date\((\d+).*/.exec(data.Result.Time)[1]));
                $('#missionlog tbody').append('<tr>' +
                                  '<td>' + toEventTime(data.Result.Time) + '</td>' +
                                  '<td>' + entry.val() + '</td>' +
                                  '<td>' + member.val() + '</td>' +
                                  '</tr>');
                <% if (canEdit) { %>
                  $('#missionlog tbody tr:last').append(
                  $('<td></td>').append(
                    $('<a href="'+shortRef+'/editlog/'+data.Result.Id+'">Edit</a>').click(function() {
                      window.open(shortRef + '/editlog/' + data.Result.Id, 'edit', 'width=540,height=450,scrollbars=1');
                      return false;
                    })).append(" ").append(
                    $('<a href="'+shortRef+'/deletelog/'+data.Result.Id+'">Delete</a>').click(function() {
                      window.open(shortRef + '/deletelog/' + data.Result.Id, 'delete', 'width=540,height=400,scrollbars=1');
                      return false;
                    }))
                    );
                <% } %>
                d.dialog('close');
                $("#missionlog").trigger("applyWidgets")
              }
              else {
                for (var i in data.Errors)
                {
                  var error = data.Errors[i];
                  var ctrl = null;
                  switch (error.Property)
                  {
                    case 'Data' : ctrl = entry; break;
                    case 'Time' : ctrl = time; break;
                    case 'Person' : ctrl = member; break;
                    default: alert(error.Property);
                  }
                  
                  if (ctrl != null)
                  {
                    $('[for='+error.Property.toLowerCase()+']').append('<span style="color:red; margin-left:1em;">'+error.Error+'</span>');
                    ctrl.addClass('ui-state-error');
                  }
                  else
                  {
                    tips.append('<span style="color:red; margin-left:1em;">'+error.Property + ': ' + error.Error+'</span>');
                    tips.addClass('ui-state-error');
                  }
                }
              }
            },
            error: function(request, status, error) {
              alert('Error submitting request:\n  ' + status + '::' + error);
            },
            complete: function(request, status) {
              $('.ui-dialog-buttonpane button').removeAttr('disabled').removeClass('ui-state-disabled');            
            }
          });
        },
        Cancel: function() {
          $(this).dialog('close');
        }
      },
      close: function() {
        volatileFields.val('');
        allFields.removeClass('ui-state-error');
        tips.removeClass('ui-state-error');
        $('label > span').remove();
        $('#addlog').focus();
      }
    });
    $('#addlog')
            .click(function() {
              time.val(formatDateTime('yy-mm-dd HH:ii', new Date()));
              $('#logform').dialog('open');
              entry.focus();
              $('#logform').dialog('option', 'position', 'center');
            });
  });
</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
