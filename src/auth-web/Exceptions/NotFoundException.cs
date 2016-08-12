/*
 * Copyright Matthew Cosand
 */
namespace Sar
{
  public class NotFoundException : UserErrorException
  {
    public NotFoundException(string message, string objectType, string objectKey) : base(message, string.Format("Could not find {0} with key {1}", objectType, objectKey)) { }
  }
}
