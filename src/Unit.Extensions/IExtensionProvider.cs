namespace Kcsara.Database.Extensions
{
  using Kcsar.Database.Model;

  [ExtensionInterface]
  public interface IExtensionProvider
  {
    void Initialize();
    T For<T>(SarUnit unit);
  }
}
