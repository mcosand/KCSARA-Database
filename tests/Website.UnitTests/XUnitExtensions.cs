using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Internal.Website
{
  public static class XAssert
  {
    public static void Equals(object left, object right, string message)
    {
      Assert.True(Equals(left, right), string.Format("{0} =? {1}\n{2}", left, right, message));
    }
  }
}
