using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kcsar.Database.Model
{
    using Microsoft.SqlServer.Types;

    public interface IAddress
    {
        string Street { get; set;  }
        string City { get; set; }
        string State { get; set; }
        string Zip { get; set; }
    }

    public interface IRefinableAddress : IAddress
    {
        int Quality { get; set; }
    }

    public interface IAddressGeography : IRefinableAddress
    {
        SqlGeography Location { get; set; }
    }
}
