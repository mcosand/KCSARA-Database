using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using Ninject.Modules;

namespace Kcsara.Database.Services
{
  public class DIModule : NinjectModule
  {
    public override void Load()
    {
      Bind<Func<IKcsarContext>>().ToConstant((Func<IKcsarContext>)(() => new KcsarContext())).InSingletonScope();
    }
  }
}
