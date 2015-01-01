<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ExpandedRowsContext>" %>
<%@ Import Namespace="Kcsar.Database.Model" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<%@ Import Namespace="Kcsara.Database.Web.Model" %>

<script type="text/javascript">
function OnSelected(id) {
  var list = jQuery("#unit_"+id);
  list.disabled = true;
  var oldColor = list.css("backgroundColor");
  list.css('backgroundColor', "Orange");
 // $("#statusprogress").css({visibility:"visible"});
  var url = '<%= Url.Action("GetMembersActiveUnits", "Members") %>' + '/' + jQuery("#pid_"+id).val()+'?when=<%= Model.RosterStart.ToString("yyyy-MM-dd") %>';

  jQuery.getJSON(url, null, function(data) {
    var result = eval(data);
    var options = '';
    
    var lookup = new Object();
    for (var i=0; i < list[0].options.length; i++)
    {
      if (list[0].options[i].text[0] != '!')
      {
        list[0].options[i].text = '! ' + list[0].options[i].text;
      }
      lookup[list[0].options[i].value.toLowerCase()] = list[0].options[i];
    }
    
    var selected = false;
    for (var i = 0; i < result.length; i++) {
      var opt = lookup[result[i].Id.toLowerCase()];
      opt.text = opt.text.substring(2);
      if (!selected)
      {
        list[0].selectedIndex = opt.index;
        selected = true;
      }
    }
  //  jQuery("#statusprogress").css({ visibility: "hidden" });
    list.css('backgroundColor', oldColor);
    list.disabled = false;
  });
  
  // Update list of animals
  var animalList = jQuery("#animal_"+id);
  var animalDiv = jQuery("#animalDiv_"+id);
  animalList.disabled = true;
  var oldAnimalColor = animalList.css("backgroundColor");
  animalList.css('backgroundColor', "Orange");
  
  jQuery.getJSON('<%= Url.Action("GetMembersAnimals", "Members") %>' + '/' + jQuery("#pid_"+id).val()+'?when=<%= Model.RosterStart.ToString("yyyy-MM-dd") %>', null, function(animalData) {
    var animalResult = eval(animalData);
    var optionsHtml = "";

    if (animalResult.length == 0)
    {
        animalDiv.css({ display: "none" });
    }
    else
    {
      for (var i = 0; i < animalResult.length; i++) {
        optionsHtml += '<option value="' + animalResult[i].Id + '">[' + animalResult[i].Type + '] ' + animalResult[i].Name + '</option>';
      }
      animalDiv.css({ display: "block" });
    }

    animalList.html(optionsHtml);
    animalList.css('backgroundColor', oldAnimalColor);
    animalList.disabled = false;
    
  });
}
</script>