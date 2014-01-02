using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using NUnit.Framework;

namespace Kcsar.Database.Model.Tests
{
    [TestFixture]
    public class ClassTests
    {
        private static Type[] IgnoredTypes = new[] { typeof(ReportingAttribute), typeof(MemberReportingAttribute) };

        [Test]
        public void NavigationPropertiesAreVirtual()
        {
            Assembly codefirstAssembly = typeof(KcsarContext).Assembly;

            foreach (Type type in codefirstAssembly.GetExportedTypes().OrderBy(f => f.Name))
            {
                if (type.IsInterface) continue;

                foreach (PropertyInfo propInfo in type.GetProperties().OrderBy(f => f.Name))
                {
                    if (propInfo.PropertyType.Assembly != codefirstAssembly) continue;
                    if (propInfo.PropertyType.IsEnum) continue;
                    if (IgnoredTypes.Contains(propInfo.PropertyType)) continue;

                    Assert.IsTrue(propInfo.GetMethod.IsVirtual, "{0}::get_{1} not virtual", type.Name, propInfo.Name);
                    Console.WriteLine("{0}::{1}", type.Name, propInfo.Name);
                }
            }
        }
    }
}
