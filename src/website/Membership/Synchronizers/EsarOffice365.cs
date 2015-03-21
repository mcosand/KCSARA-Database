/*
 * Copyright 2015 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kcsara.Database.Web.Membership.Synchronizers
{
  public class EsarOffice365 : IPasswordSynchronizerWithOptions
  {
    private Dictionary<string, string> options;
    private readonly ILog log;

    public EsarOffice365(string option, ILog log)
    {
      this.log = log;
    }

    public void SetPassword(string username, string newPassword)
    {
      try
      { 
      Task.Run(() => SetPasswordAsync(username, newPassword)).Wait();
      }
      catch (AggregateException ex)
      {
        foreach (var inner in ex.InnerExceptions)
        {
          var nested = inner;
          while (nested.InnerException != null) { nested = nested.InnerException; }
          if (nested.Message.StartsWith("{"))
          {
            var jObject = JObject.Parse(nested.Message);
            if (jObject["odata.error"] != null && jObject["odata.error"]["message"] != null && jObject["odata.error"]["message"]["value"] != null)
            {
              throw new ApplicationException(jObject["odata.error"]["message"]["value"].ToString(), nested);
            }
          }
          throw nested;
        }
      }
    }

    public async Task SetPasswordAsync(string username, string newPassword){
      ActiveDirectoryClient activeDirectoryClient;
      try
      {
        activeDirectoryClient = GetActiveDirectoryClientAsApplication();
      }
      catch (AuthenticationException ex)
      {
        log.Error("Error authenticating to Office365", ex);
        throw new ApplicationException("Error connecting to Office 365");
      }

      List<IUser> usersList = null;
      IPagedCollection<IUser> searchResults = null;
      IUserCollection userCollection = activeDirectoryClient.Users;
      searchResults = userCollection.Where(user => user.UserPrincipalName.Equals(username + "@kcesar.org")).ExecuteAsync().Result;
      usersList = searchResults.CurrentPage.ToList();

      if (usersList != null && usersList.Count == 1)
      {
        do
        {
          usersList = searchResults.CurrentPage.ToList();
          foreach (IUser user in usersList)
          {
            this.log.DebugFormat("Can't set password for {0} - not an Office365 user.", username);
            user.PasswordProfile = new PasswordProfile
            {
              Password = newPassword,
              ForceChangePasswordNextLogin = false
            };
            await user.UpdateAsync();
          }
          searchResults = searchResults.GetNextPageAsync().Result;
        } while (searchResults != null);
      }
      else
      {
        this.log.DebugFormat("Can't set password for {0} - not an Office365 user.", username);
      }
    }

    public void SetOptions(Dictionary<string, string> options)
    {
      this.options = options;
    }

    public async Task<string> AcquireTokenAsyncForApplication()
    {
      string result = await Task.Factory.StartNew<string>(() => GetTokenForApplication());
      return result;
    }

    public string GetTokenForApplication()
    {
      AuthenticationContext authenticationContext = new AuthenticationContext(options["authString"], false);
      // Config for OAuth client credentials 
      ClientCredential clientCred = new ClientCredential(options["clientId"], options["clientSecret"]);
      AuthenticationResult authenticationResult = authenticationContext.AcquireToken(options["resourceUrl"], clientCred);
      string token = authenticationResult.AccessToken;
      return token;
    }

    public ActiveDirectoryClient GetActiveDirectoryClientAsApplication()
    {
      Uri servicePointUri = new Uri(options["resourceUrl"]);
      Uri serviceRoot = new Uri(servicePointUri, options["tenantId"]);
      ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot, async () => await AcquireTokenAsyncForApplication());
      return activeDirectoryClient;
    }

    public string Name
    {
      get { return "Office 365"; }
    }
  }
}