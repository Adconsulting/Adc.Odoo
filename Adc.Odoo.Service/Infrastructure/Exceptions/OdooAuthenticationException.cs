using System;

namespace Adc.Odoo.Service.Infrastructure.Exceptions
{
    [Serializable]
    public class OdooAuthenticationException : OdooException
    {
        public OdooAuthenticationException()
        {
            
        }

        public OdooAuthenticationException(string message) : base (message)
        {  
        }

        public OdooAuthenticationException(string message, Exception innerException): base (message, innerException)
        {
            
        }
    }
}
