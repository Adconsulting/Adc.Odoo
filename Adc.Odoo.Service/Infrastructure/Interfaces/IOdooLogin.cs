using CookComputing.XmlRpc;

namespace Adc.Odoo.Service.Infrastructure.Interfaces
{
    public interface IOdooLogin : IXmlRpcProxy
    {
        [XmlRpcMethod("authenticate")]
        object Login(string database, string username, string password, object param);
    }
}
