namespace Internal.Website
{
  using System.Collections.Specialized;
  using Kcsar.Database.Data;

  public class IntegrationContext
  {
    public string DataConnectionString { get; set; }
    public string AuthConnectionString { get; set; }
    public StringDictionary KnownUsers { get; set; }
    public string AdminUser { get; set; }
    public string Url { get; set; }

    public KcsarContext GetDb()
    {
      return new KcsarContext(this.DataConnectionString);
    }

    public static IntegrationContext Load()
    {
      IntegrationContext context = new IntegrationContext
      {
        Url = "http://localhost:4944",
        AdminUser = "admin",
        KnownUsers = new StringDictionary {{ "admin", "password"}},
        AuthConnectionString = "Data Source=(localdb)\\v11.0;Initial Catalog=devdb;Persist Security Info=True;Integrated Security=true",
        DataConnectionString = "Data Source=(localdb)\\v11.0;Initial Catalog=devdb;Persist Security Info=True;Integrated Security=true;MultipleActiveResultSets=true"
      };
      return context;
    }
  }
}
