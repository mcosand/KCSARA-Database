/*
 * Copyright 2015 Matthew Cosand
 */
namespace Kcsara.Database.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using Kcsar.Database.Data;
  using Kcsara.Database.Model.Members;
  using log4net;

  public interface IMembersService
  {
    List<MemberSummary> Search(string term);
    string PhotoPath(Guid id);
    MemberSummary Summary(Guid id);
    List<MemberContact> ContactList(Guid id);
    List<MemberAddress> AddressList(Guid id);
  }

  public class MembersService : BaseDataService, IMembersService
  {
    /// <summary>Default Constructor</summary>
    /// <param name="dbFactory"></param>
    /// <param name="log"></param>
    public MembersService(Func<IKcsarContext> dbFactory, ILog log)
      : base(dbFactory, log)
    {
    }

    protected Expression<Func<MemberRow, MemberSummary>> toDomainSummary = row => new MemberSummary
    {
      Id = row.Id,
      Name = row.LastName + ", " + row.FirstName,
      IdNumber = row.DEM,
      HasPhoto = row.PhotoFile != null,
      CardType = row.WacLevel
    };
    protected Expression<Func<MemberContactRow, MemberContact>> toDomainContact = row => new MemberContact
    {
      Id = row.Id,
      Priority = row.Priority,
      Type = row.Type,
      SubType = row.Subtype,
      Value = row.Value
    };
    protected Expression<Func<MemberAddressRow, MemberAddress>> toDomainAddress = row => new MemberAddress
    {
      Id = row.Id,
      Type = row.InternalType,
      Street = row.Street,
      City = row.City,
      State = row.State,
      Zip = row.Zip
    };

    public List<MemberSummary> Search(string query)
    {
      using (var db = dbFactory())
      {
        return (from m in db.Members
                where (m.FirstName + " " + m.LastName).ToLower().Contains(query)
                  || (m.LastName + ", " + m.FirstName).ToLower().Contains(query)
                  || m.DEM.Contains(query)
                select m).OrderBy(f => f.LastName).ThenBy(f => f.FirstName)
                .Select(toDomainSummary)
                .ToList();
      }
    }

    public MemberSummary Summary(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.Members.Where(f => f.Id == id).Select(toDomainSummary).FirstOrDefault();
      }
    }

    public string PhotoPath(Guid id)
    {
      using (var db = dbFactory())
      {
        return db.Members.Where(f => f.Id == id).Select(f => f.PhotoFile).FirstOrDefault();
      }
    }

    /// <summary>Get a list of Members</summary>
    /// <returns></returns>
    public List<MemberSummary> List()
    {
      //using (var db = this.dbFactory())
      //{
      //  return db.Members.OrderBy(f => f.DisplayName).Select(toDomainSummary).ToList();
      //}
      return null;
    }


    public List<MemberContact> ContactList(Guid id)
    {
      using (var db = this.dbFactory())
      {
        return db.Members.Where(f => f.Id == id)
          .SelectMany(f => f.ContactNumbers)
          .OrderBy(f => f.Type).ThenBy(f => f.Priority).ThenBy(f => f.Subtype)
          .Select(toDomainContact)
          .ToList();
      }
    }

    public List<MemberAddress> AddressList(Guid id)
    {
      using (var db = this.dbFactory())
      {
        return db.Members.Where(f => f.Id == id)
          .SelectMany(f => f.Addresses)
          .OrderBy(f => f.InternalType)
          .Select(toDomainAddress)
          .ToList();
      }
    }
  }
}
