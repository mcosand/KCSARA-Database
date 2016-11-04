using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Sar.Database.Services;

namespace Kcsara.Database.Api.Controllers.Accounts
{
  public class RolesController : ApiController
  {
    private readonly IRolesService _roles;

    public RolesController(IRolesService roles)
    {
      _roles = roles;
    }


  }
}
