/*
 * Copyright 2013-2014 Matthew Cosand
 */

namespace Kcsara.Database.Web.api
{
  using System;
  using System.Collections.Generic;
  using System.Web.Http;
  using Kcsara.Database.Model.Members;
  using Kcsara.Database.Services;
  using log4net;

  [ModelValidationFilter]
  public class MemberAddressesController : NoDataBaseApiController
  {
    protected readonly IMembersService membersSvc;

    public MemberAddressesController(IMembersService membersSvc, ILog log)
      : base(log)
    {
      this.membersSvc = membersSvc;
    }

    [HttpGet]
    [Authorize(Roles = "cdb.users")]
    public List<MemberAddress> List(Guid id)
    {
      return this.membersSvc.AddressList(id);
    }
  }
}
