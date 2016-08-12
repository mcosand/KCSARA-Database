/*
 * Copyright Matthew Cosand
 */
namespace Sar
{
  using System;

  public class UserErrorException : ApplicationException
  {
    public string ExternalMessage { get; private set; }

    public UserErrorException(string externalMessage) : this(externalMessage, externalMessage) { }
    public UserErrorException(string externalMessage, string internalMessage) : base(internalMessage)
    {
      ExternalMessage = externalMessage;
    }
    public UserErrorException(string externalMessage, Exception innerException) : base(innerException.Message, innerException)
    {
      ExternalMessage = externalMessage;
    }
  }
}
