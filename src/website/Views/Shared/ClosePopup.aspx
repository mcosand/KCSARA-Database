<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Kcsara.Database.Web.Controllers" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <script type="text/javascript">
      if (opener) {
        <%= (ViewState["OpenerAction"] != null) ? (string)ViewState["OpenerAction"] : "opener.window.location = opener.window.location;" %>
      }
      <%= (Request.Browser.Browser == "IE") ? "window.open('" + Url.Action("Close") + "', '_self');" : "window.close();" %>
    </script>
</head>
<body>
Please close this window.
</body>
</html>
