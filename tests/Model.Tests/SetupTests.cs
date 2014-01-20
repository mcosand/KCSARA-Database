/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using NUnit.Framework;

namespace Internal.Database.Model
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
