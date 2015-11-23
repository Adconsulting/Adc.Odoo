using System;

using CookComputing.XmlRpc;

namespace Adc.Odoo.Service.Infrastructure.Interfaces
{
    public interface IOdoo : IXmlRpcProxy
    {
        [XmlRpcMethod("execute")]
        int Create(string dbName, int userId, string pwd, string model, string method, XmlRpcStruct fieldValues);

        [XmlRpcMethod("execute")]
        bool Write(string dbName, int userId, string pwd, string model, string method, int[] ids, XmlRpcStruct fieldValues);

        [XmlRpcMethod("execute")]
        Object[] Search(string dbName, int userId, string pwd, string model, string method, Object[] filters);

        [XmlRpcMethod("execute")]
        Object[] Search(string dbName, int userId, string pwd, string model, string method, Object[] filters, int offset, int limit);

        [XmlRpcMethod("execute")]
        Object[] Search(string dbName, int userId, string pwd, string model, string method, Object[] filters, int offset, int limit, string order);

        [XmlRpcMethod("execute")]
        Object[] Read(string dbName, int userId, string dbPwd, string model, string method, object[] ids, object[] fields);

        [XmlRpcMethod("execute")]
        bool Unlink(string dbName, int userId, string dbPwd, string model, string method, object[] ids);

    }
}
