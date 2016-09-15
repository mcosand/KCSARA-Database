/*
 * Copyright Matthew Cosand
 */
namespace Sar
{
  public class MultipleMatchesException : UserErrorException
  {
    public MultipleMatchesException(string message, string objectType, string objectKey) : base(message, string.Format("Found multiple matching {0} with key {1}", objectType, objectKey)) { }
  }
}
