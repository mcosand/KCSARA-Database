﻿using Microsoft.AspNet.Razor.TagHelpers;

namespace Kcsara.Database.Web.TagHelpers
{
  [HtmlTargetElement("li", Attributes = MenuActiveAttribute)]
  public class MenuActive : TagHelper
  {
    private const string MenuActiveAttribute = "sar-menu-name";

    ///// <summary>
    ///// An expression to be evaluated against the current model.
    ///// </summary>
    [HtmlAttributeName(MenuActiveAttribute)]
    public string MenuName { get; set; }

    [HtmlAttributeName("sar-active-menu")]
    public string ActiveName { get; set; }

    [HtmlAttributeName("sar-add-classes")]
    public string AddClasses { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
      var activePath = ((string)(ActiveName) ?? string.Empty);

      output.Attributes["class"] = AddClasses
                                      + (AddClasses.Length > 0 ? " " : string.Empty)
                                      + ((activePath + '/').ToUpperInvariant().StartsWith(MenuName.ToUpperInvariant() + '/') ? " active" : string.Empty);
    }
  }
}