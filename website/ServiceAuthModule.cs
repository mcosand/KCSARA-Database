
namespace Kcsara.Database.Web
{
    using System;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.Security;

    /// <summary>
    /// HttpModule that provides Basic authentication for asp.net applications
    /// </summary>
    public class ServiceAuthModule : IHttpModule
    {
        #region IHttpModule Members

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += new EventHandler(context_AuthenticateRequest);
            context.AuthorizeRequest += new EventHandler(context_AuthorizeRequest);
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        /// <summary>
        /// Handles the BeginRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication context = sender as HttpApplication;

            if (IsRequestingBasic(context))
            {
                if (string.IsNullOrEmpty(GetAuthHeader(context)))
                {
                    SendAuthHeader(context);
                    return;
                }
            }
        }

        private bool IsRequestingBasic(HttpApplication context)
        {
            return string.Equals(context.Request.Params["_auth"], "basic", StringComparison.OrdinalIgnoreCase);
        }

        //private bool IsRequestingPostAuth(HttpApplication context)
        //{
        //    return !string.IsNullOrWhiteSpace(context.Request.Form["_username"]);
        //}

        private string GetAuthHeader(HttpApplication context)
        {
            return context.Request.Headers["Authorization"];
        }

        /// <summary>
        /// Sends the Unauthorized header to the user, telling the user to provide a valid username and password
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendAuthHeader(HttpApplication context)
        {
            context.Response.Clear();
            context.Response.StatusCode = 401;
            context.Response.StatusDescription = "Unauthorized";
            context.Response.AddHeader("WWW-Authenticate", "Basic realm=\"Secure Area\"");
            context.Response.Write("401 baby, please authenticate");
            context.Response.End();
        }

        /// <summary>
        /// Handles the AuthorizeRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void context_AuthorizeRequest(object sender, EventArgs e)
        {
            HttpApplication context = sender as HttpApplication;
        }

        /// <summary>
        /// Sends the not authorized headers to the user
        /// </summary>
        /// <param name="context">The context.</param>
        private void SendNotAuthorized(HttpApplication context)
        {
            context.Response.Clear();
            context.Response.StatusCode = 403;
            context.Response.StatusDescription = "Forbidden";
            context.Response.Write("403 Forbidden");
            context.Response.End();
        }

        /// <summary>
        /// Tries to authenticate the user
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private bool TryAuthenticate(HttpApplication context)
        {
            string authHeader = GetAuthHeader(context);
            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("basic ", StringComparison.InvariantCultureIgnoreCase))
                {
                    string userNameAndPassword = Encoding.Default.GetString(
                        Convert.FromBase64String(authHeader.Substring(6)));

                    string[] parts = userNameAndPassword.Split(':');

                    if (System.Web.Security.Membership.ValidateUser(parts[0], parts[1]))
                    {
                        FormsAuthentication.SetAuthCookie(parts[0], true);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handles the AuthenticateRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void context_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication context = sender as HttpApplication;
            if (IsRequestingBasic(context))
            {
                HttpCookie cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null)
                {
                    FormsIdentity fi = new FormsIdentity(FormsAuthentication.Decrypt(cookie.Value));
                    context.Context.User = new RolePrincipal(fi);
                    // Already authenticated with FormsAuth
                }
                else
                {
                    string authHeader = GetAuthHeader(context);
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        if (authHeader.StartsWith("basic ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string userNameAndPassword = Encoding.Default.GetString(
                                Convert.FromBase64String(authHeader.Substring(6)));

                            string[] parts = userNameAndPassword.Split(':');

                            DoAuth(context, parts[0], parts[1]);
                        }
                    }
                }
            }
            //else if (IsRequestingPostAuth(context))
            //{
            //    DoAuth(context, context.Request.Form["_username"], context.Request.Form["_password"]);
            //}
        }

        private static void DoAuth(HttpApplication context, string username, string password)
        {
            if (System.Web.Security.Membership.ValidateUser(username, password))
            {
                FormsAuthentication.SetAuthCookie(username, true);
                HttpCookie tmpCookie = FormsAuthentication.GetAuthCookie(username, true);
                context.Context.User = new RolePrincipal(new FormsIdentity(FormsAuthentication.Decrypt(tmpCookie.Value)));
            }
        }
        #endregion
    }
}
