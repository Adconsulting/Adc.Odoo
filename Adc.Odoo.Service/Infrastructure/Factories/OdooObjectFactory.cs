using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Adc.Odoo.Service.Infrastructure.Attributes;
using Adc.Odoo.Service.Infrastructure.Enums;
using Adc.Odoo.Service.Infrastructure.Exceptions;
using Adc.Odoo.Service.Infrastructure.Interfaces;
using Adc.Odoo.Service.Models;

using CookComputing.XmlRpc;

namespace Adc.Odoo.Service.Infrastructure.Factories
{
    public class OdooObjectFactory
    {
        /// <summary>
        /// Create entities and load values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="resultset"></param>
        /// <returns></returns>
        public static ICollection<T> BuildEntities<T>(OdooService service, ResultSet resultset) where T : IOdooObject, new()
        {
            var entityCollection = new OdooCollection<T>(service);
            BuildEntities(service, resultset, entityCollection);

            return entityCollection;
        }

        /// <summary>
        /// Use an existint collection to load entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="resultset"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static void BuildEntities<T>(OdooService service, ResultSet resultset, ICollection<T> collection) where T : IOdooObject, new()
        {
            foreach (object entity in resultset.Data)
            {
                var xmlStruct = (XmlRpcStruct)entity;
                collection.Add(BuildObject<T>(service, xmlStruct));
            }
        }

        private static T BuildObject<T>(OdooService service, XmlRpcStruct xmlStruct) where T : IOdooObject, new()
        {
            var obj = new T();

            var props = obj.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(OdooMapAttribute), true).Any());

            var fkProps = obj.GetType()
                           .GetProperties()
                           .Where(p => p.GetCustomAttributes(typeof(OdooForeignKeyAttribute), true).Any());

            foreach (var propertyInfo in props)
            {
                var info = propertyInfo.GetCustomAttribute<OdooMapAttribute>();

                if (xmlStruct.ContainsKey(info.OdooName))
                {
                    var response = xmlStruct[info.OdooName];
                    object value;
                    if (IsOdooNull(response))
                    {
                        value = null;
                    }
                    else if (response as object[] != null)
                    {
                        var val = (response as object[]).Length > 0 ? (response as object[])[0] : null;
                        //CHECK FOR FK MAPPING
                        var fkProp = fkProps.FirstOrDefault(
                            x => x.GetCustomAttribute<OdooForeignKeyAttribute>()
                                     .PropertyName == info.OdooName);
                        if (fkProp != null)
                        {
                            if (val != null)
                            {

                                var type = typeof(OdooObject<>).MakeGenericType(fkProp.PropertyType);
                                var @object = Activator.CreateInstance(type, service, val);
                                var fkval = @object.GetType()
                                    .GetMethod("GetObject")
                                    .Invoke(@object, null);
                                if (fkval != null)
                                    fkProp.SetValue(obj, fkval);
                            }
                            else
                            {
                                value = null;
                            }

                        }

                        if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                        {
                            value = val;
                        }
                        else
                        {
                            if (val != null)
                            {
                                var type = typeof(OdooObject<>).MakeGenericType(propertyInfo.PropertyType);
                                var @object = Activator.CreateInstance(type, service, val);
                                value = @object.GetType()
                                    .GetMethod("GetObject")
                                    .Invoke(@object, null);
                            }
                            else
                            {
                                value = null;
                            }
                        }
                    }
                    else if (response as int[] != null)
                    {
                        var attibute = propertyInfo.PropertyType.GenericTypeArguments.First()
                            .CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(OdooMapAttribute));
                        if (attibute != null)
                        {
                            var type = typeof(OdooCollection<>).MakeGenericType(propertyInfo.PropertyType.GetGenericArguments());
                            value = Activator.CreateInstance(type, service, response);
                        }
                        else
                        {
                            value = response;
                        }
                    }
                    else
                    {
                        switch (info.OdooType)
                        {
                            case OdooType.Date:
                                value = DateTime.ParseExact(response.ToString(), "yyyy-MM-dd", System.Threading.Thread.CurrentThread.CurrentCulture);
                                break;
                            case OdooType.Datetime:
                                value = DateTime.ParseExact(response.ToString(), "yyyy-MM-dd HH:mm:ss", System.Threading.Thread.CurrentThread.CurrentCulture);
                                break;
                            case OdooType.Integer:
                                value = int.Parse(response.ToString());
                                break;
                            case OdooType.Float:
                                value = Decimal.Parse(response.ToString());
                                break;
                            case OdooType.Char:
                                if (propertyInfo.Name.Equals("DateTime"))
                                {
                                    value = DateTime.ParseExact(
                                        response.ToString(),
                                        "yyyy-MM-dd HH:mm:ss",
                                        System.Threading.Thread.CurrentThread.CurrentCulture);
                                }
                                else
                                {
                                    value = Convert.ChangeType(response, propertyInfo.PropertyType);
                                }
                                break;
                            default:
                                value = Convert.ChangeType(response.ToString(), propertyInfo.PropertyType);
                                break;
                        }
                    }
                    propertyInfo.SetValue(obj, value);
                }
                else
                {
                    //throw new OdooException(string.Format("Field {0} not found in server response", info.OdooName));
                    propertyInfo.SetValue(obj, null);
                }

            }
            return obj;
        }

        private static void BuildSubOject<T>(OdooService service, T obj, OdooMapAttribute propInfo)
        {
            var props = obj.GetType()
               .GetProperties()
               .Where(p => p.GetCustomAttributes(typeof(OdooForeignKeyAttribute), true).Any());

            var objProp = props.FirstOrDefault(
                x => x.GetCustomAttribute<OdooForeignKeyAttribute>()
                         .PropertyName == propInfo.OdooName);
            if (objProp != null)
            {

            }
        }

        /// <summary>
        /// Checks for a null value from OpenErp.
        /// OpenErp will return false for a null value.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsOdooNull(object value)
        {
            if (value is bool && !(bool)value)
            {
                return true;
            }
            return false;
        }



    }
}
