using CookComputing.XmlRpc;

namespace Adc.Odoo.Service.Infrastructure.Interfaces
{
    public interface IOdooDatabase : IXmlRpcProxy
    {
        [XmlRpcMethod("list")]
        object List();
    }
}
