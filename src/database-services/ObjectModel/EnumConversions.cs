using Kcsara.Database.Model;
using Data = Kcsar.Database.Model;

namespace Kcsara.Database.Services
{
  public static class EnumConversionExtensions
  {
    public static Gender ToModel(this Data.Gender dataGender)
    {
      return (Gender)((int)dataGender);
    }

    public static Data.Gender FromModel(this Gender gender)
    {
      return (Data.Gender)((int)gender);
    }

    public static WacLevel ToModel(this Data.WacLevel dataGender)
    {
      return (WacLevel)((int)dataGender);
    }

    public static Data.WacLevel FromModel(this WacLevel gender)
    {
      return (Data.WacLevel)((int)gender);
    }
  }
}
