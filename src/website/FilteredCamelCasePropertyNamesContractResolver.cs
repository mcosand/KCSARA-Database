namespace Kcsara.Database.Web
{
  using System;
  using System.Reflection;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Serialization;

  /// <summary>SignalR base classes expect PascalCased properties, but all of our other UI code expects camelCased properties.
  /// Do automatic camelCase conversion based on class namespace.</summary>
  public class FilteredCamelCasePropertyNamesContractResolver : DefaultContractResolver
  {
    private readonly string namespaceFilter;
    public FilteredCamelCasePropertyNamesContractResolver(string namespaceFilter)
    {
      this.namespaceFilter = namespaceFilter;
    }
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
      var jsonProperty = base.CreateProperty(member, memberSerialization);
      Type declaringType = member.DeclaringType;
      if (declaringType.FullName.StartsWith(namespaceFilter))
      {
        jsonProperty.PropertyName = jsonProperty.PropertyName.ToCamelCase();
      }
      return jsonProperty;
    }
  }   
}