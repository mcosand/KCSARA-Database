namespace Internal.Database.Model.Setup
{
  using System;
  using System.IO;
  using System.Xml;
  using Kcsar.Database.Model;

  internal static class ContextGenerator
  {
    internal static KcsarContext CreateContext()
    {
      XmlDocument config = new XmlDocument();
      string webConfigPath = Path.Combine(Path.GetDirectoryName(new Uri(typeof(MissionResponse).Assembly.CodeBase).LocalPath), "..\\..\\..\\..\\website\\web.config");
      config.Load(webConfigPath);

      var connString = config.SelectSingleNode("//connectionStrings/add[@name='DataStore']").Attributes["connectionString"].Value;
      Console.WriteLine(connString);
      return new KcsarContext(connString);
    }
  }
}
