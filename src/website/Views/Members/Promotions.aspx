<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>View Hours/Promotions</h2>

    <input name="type" type="radio" class="selectType" value="abs" checked="checked" /> Absolute dates<br />
    <input type="radio" class="selectType" name="type" value="rel" /> Relative dates (everyone starts at the same time)<br />
    <!-- <input type="radio" name="type" class="selectType" value="start" /> Start at date: -->

<div id="people">

</div>
<input type="button" id="addName" value="Add Member" /><br />
<input type="button" id="link" value="View Graph" disabled="disabled" />

<div id="info"></div>
<script type="text/javascript">
  var graphHref = '';

  function updateLink() {
    var href = '';
    var hasPeople = false;

    var type = $(".selectType:checked").val();
    if (type == "rel") {
      href += "&relative=true";
    }

    $(".suggest").each(function () {
      if (this.person != null && this.value != '') { hasPeople = true; href += "&m=" + this.person.Id; }
    });

    graphHref = window.location.href.replace(/promotions.*/, "promotionsresult?") + href.substring(1);
    if (hasPeople)
    {
      $('#link').removeAttr('disabled');
    }
    else
    {
      $('#link').attr('disabled', 'disabled');
    }
  }

  $(document).ready(function () {
    $('#addName').click(function () {
      var ppl = $('#people');
      var box = $('<input type="text" class="suggest" style="margin-bottom:.5em" id="member' + ppl[0].children.length + '" />').appendTo(ppl).suggest3("<%= Html.UrlFrom<MembersController>(x => x.Suggest(null)) %>", { onSelect: updateLink });
      $("<br/>").appendTo(ppl);
    });
    $('.selectType').click(function () { updateLink() });
    $('#link').click(function () { updateLink(); window.open(graphHref, "graph"); });
  });



</script>

</asp:Content>