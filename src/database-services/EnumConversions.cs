using DB = Kcsar.Database.Model;

namespace Sar.Database.Model
{
  public static class EnumConversionExtensions
  {
    public static Gender ToModel(this DB.Gender dataGender)
    {
      return (Gender)((int)dataGender);
    }

    public static DB.Gender FromModel(this Gender gender)
    {
      return (DB.Gender)((int)gender);
    }

    public static WacLevel ToModel(this DB.WacLevel dataGender)
    {
      return (WacLevel)((int)dataGender);
    }

    public static DB.WacLevel FromModel(this WacLevel gender)
    {
      return (DB.WacLevel)((int)gender);
    }
  }
}
