/*
 * Copyright 2013-2014 Matthew Cosand
 */
namespace Internal.Website
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using log4net;

  public class ConsoleLogger : ILog
  {
    public void Debug(object message, Exception exception)
    {
      Console.WriteLine("DEBUG {0}\n{1}", message, exception);
    }

    public void Debug(object message)
    {
      Console.WriteLine("DEBUG {0}", message);
    }

    public void DebugFormat(IFormatProvider provider, string format, params object[] args)
    {
      Console.WriteLine(string.Format(provider, format, args));
    }

    public void DebugFormat(string format, object arg0, object arg1, object arg2)
    {
      Console.WriteLine("DEBUG {0}", string.Format(format, arg0, arg1, arg2));
    }

    public void DebugFormat(string format, object arg0, object arg1)
    {
      Console.WriteLine("DEBUG {0}", string.Format(format, arg0, arg1));
    }

    public void DebugFormat(string format, object arg0)
    {
      Console.WriteLine("DEBUG {0}", string.Format(format, arg0));
    }

    public void DebugFormat(string format, params object[] args)
    {
      Console.WriteLine("DEBUG {0}", string.Format(format, args));
    }

    public void Error(object message, Exception exception)
    {
      Console.WriteLine("ERROR {0}\n{1}", message, exception);
    }

    public void Error(object message)
    {
      Console.WriteLine("ERROR {0}", message);
    }

    public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
    {
      Console.WriteLine(string.Format(provider, format, args));
    }

    public void ErrorFormat(string format, object arg0, object arg1, object arg2)
    {
      Console.WriteLine("ERROR {0}", string.Format(format, arg0, arg1, arg2));
    }

    public void ErrorFormat(string format, object arg0, object arg1)
    {
      Console.WriteLine("ERROR {0}", string.Format(format, arg0, arg1));
    }

    public void ErrorFormat(string format, object arg0)
    {
      Console.WriteLine("ERROR {0}", string.Format(format, arg0));
    }

    public void ErrorFormat(string format, params object[] args)
    {
      Console.WriteLine("ERROR {0}", string.Format(format, args));
    }

    public void Fatal(object message, Exception exception)
    {
      Console.WriteLine("FATAL {0}\n{1}", message, exception);
    }

    public void Fatal(object message)
    {
      Console.WriteLine("FATAL {0}", message);
    }

    public void FatalFormat(IFormatProvider provider, string format, params object[] args)
    {
      Console.WriteLine(string.Format(provider, format, args));
    }

    public void FatalFormat(string format, object arg0, object arg1, object arg2)
    {
      Console.WriteLine("FATAL {0}", string.Format(format, arg0, arg1, arg2));
    }

    public void FatalFormat(string format, object arg0, object arg1)
    {
      Console.WriteLine("FATAL {0}", string.Format(format, arg0, arg1));
    }

    public void FatalFormat(string format, object arg0)
    {
      Console.WriteLine("FATAL {0}", string.Format(format, arg0));
    }

    public void FatalFormat(string format, params object[] args)
    {
      Console.WriteLine("FATAL {0}", string.Format(format, args));
    }

    public void Info(object message, Exception exception)
    {
      Console.WriteLine("INFO {0}\n{1}", message, exception);
    }

    public void Info(object message)
    {
      Console.WriteLine("INFO {0}", message);
    }

    public void InfoFormat(IFormatProvider provider, string format, params object[] args)
    {
      Console.WriteLine(string.Format(provider, format, args));
    }

    public void InfoFormat(string format, object arg0, object arg1, object arg2)
    {
      Console.WriteLine("INFO {0}", string.Format(format, arg0, arg1, arg2));
    }

    public void InfoFormat(string format, object arg0, object arg1)
    {
      Console.WriteLine("INFO {0}", string.Format(format, arg0, arg1));
    }

    public void InfoFormat(string format, object arg0)
    {
      Console.WriteLine("INFO {0}", string.Format(format, arg0));
    }

    public void InfoFormat(string format, params object[] args)
    {
      Console.WriteLine("INFO {0}", string.Format(format, args));
    }
    public bool IsDebugEnabled
    {
      get { return true; }
    }

    public bool IsErrorEnabled
    {
      get { return true; }
    }

    public bool IsFatalEnabled
    {
      get { return true; }
    }

    public bool IsInfoEnabled
    {
      get { return true; }
    }

    public bool IsWarnEnabled
    {
      get { return true; }
    }

    public void Warn(object message, Exception exception)
    {
      Console.WriteLine("WARN {0}\n{1}", message, exception);
    }

    public void Warn(object message)
    {
      Console.WriteLine("WARN {0}", message);
    }

    public void WarnFormat(IFormatProvider provider, string format, params object[] args)
    {
      Console.WriteLine(string.Format(provider, format, args));
    }

    public void WarnFormat(string format, object arg0, object arg1, object arg2)
    {
      Console.WriteLine("WARN {0}", string.Format(format, arg0, arg1, arg2));
    }

    public void WarnFormat(string format, object arg0, object arg1)
    {
      Console.WriteLine("WARN {0}", string.Format(format, arg0, arg1));
    }

    public void WarnFormat(string format, object arg0)
    {
      Console.WriteLine("WARN {0}", string.Format(format, arg0));
    }

    public void WarnFormat(string format, params object[] args)
    {
      Console.WriteLine("WARN {0}", string.Format(format, args));
    }

    public log4net.Core.ILogger Logger
    {
      get { throw new NotImplementedException(); }
    }
  }
}
