using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using NUnit.Framework;

namespace Kcsar.Database.Model.Tests
{
    [TestFixture]
    public class SetupTests
    {
        [Test]
        public void SetupDatabase()
        {
            using (var context = new KcsarContext())
            {
                Console.WriteLine(context.Members.Count());
            }
        }

        
    }
}
