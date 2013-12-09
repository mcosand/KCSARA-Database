using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;

namespace Kcsara.Database.Tools
{
    public class RecalculateAwards
    {
        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;
            Dictionary<Guid, string> memberIds = null;
            using (var context = new KcsarContext())
            {
                memberIds = context.Members.OrderBy(f => f.LastName).ThenBy(f => f.FirstName).ToDictionary(f => f.Id, f => f.LastName + ", " + f.FirstName);
            }

            foreach (var pair in memberIds)
            {
                    using (var ctx = new KcsarContext())
                    {
                        Console.WriteLine(pair.Value);
                        ctx.RecalculateTrainingAwards(pair.Key, DateTime.Now);
                        ctx.SaveChanges();
                    }
            }
            Console.WriteLine("Execution time: {0}", (DateTime.Now - startTime));
        }
    }
}
