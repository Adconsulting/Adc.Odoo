using System;
using System.Linq;

using CookComputing.XmlRpc;

namespace Adc.Odoo.Service.Infrastructure.Exceptions
{
    public class OdooException : Exception
    {
        public OdooException()
        {
            
        }

        public OdooException(string message) : base(message)
        {
        }

        public OdooException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        protected internal static OdooException GetException(XmlRpcFaultException e)
        {
            string message = string.Empty;
            string[] messages = e.Message.Split('\n');
            if (messages.Length >= 3)
            {
                try
                {
                    message = string.Join("\n", messages.Skip(2));
                }
                catch (Exception)
                {
                    message = e.Message;
                }
            }
            else
            {
                message = e.Message;
            }
            return new OdooException(message, e);
        }
    }
}
