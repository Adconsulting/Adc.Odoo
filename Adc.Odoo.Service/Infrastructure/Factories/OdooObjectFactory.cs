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
                        value = (response as object[]).Length > 0 ? (response as object[])[0] : null;
                    } else if (response as int[] != null)
                    {
                        value = response;
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
                    throw new OdooException(string.Format("Field {0} not found in server response", info.OdooName));
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



        /*

                public static T BuildEntity<T>(OdooService service, XmlRpcStruct xmlStruct) where T : IOdooObject, new()
                {
                    T entity = new T();
                    foreach (DictionaryEntry item in xmlStruct)
                    {
                        PropertyInfo property = GetPropertyFromEntity<T>(item.Key as string);
                        if (property != null)
                        {
                            object value = item.Value;
                            if (IsOdooNull(item.Value))
                            {
                                //If is a basic type this throws an exception
                                value = null;
                            }
                            else if (property.PropertyType.IsGenericCollection())
                            {
                                //Create a collection of entities
                                Type type = typeof(OdooCollection<>).MakeGenericType(property.PropertyType.GetGenericArguments());
                                value = Activator.CreateInstance(type, service, item.Value);
                            }
                            else
                            {
                                var attibute = (OdooMapAttribute)property.GetCustomAttributes(typeof(OdooMapAttribute), false).FirstOrDefault();
                                if (attibute != null)
                                {
                                    switch (attibute.OdooType)
                                    {
                                        case OdooType.Char:
                                            if (property.Name.Equals("DateTime"))
                                            {
                                                value = DateTime.ParseExact(item.Value.ToString(), "yyyy-MM-dd HH:mm:ss", System.Threading.Thread.CurrentThread.CurrentCulture);
                                            }
                                            break;
                                        case OdooType.Text:
                                            break;
                                        case OdooType.Date:
                                            value = DateTime.ParseExact(item.Value.ToString(), "yyyy-MM-dd", System.Threading.Thread.CurrentThread.CurrentCulture);
                                            break;
                                        case OdooType.Datetime:
                                            value = DateTime.ParseExact(item.Value.ToString(), "yyyy-MM-dd HH:mm:ss", System.Threading.Thread.CurrentThread.CurrentCulture);
                                            break;
                                        default:
                                            value = item.Value;
                                            break;
                                    }
                                }
                            }
                            //check if is a object. Load related entity.
                            if (value as object[] != null)
                            {
                                if ((value as object[]).Length > 0)
                                {
                                    value = (value as object[])[0]; //id of related entity
                                }
                                else
                                {
                                    value = null;
                                }
                            }
                            //Asign value directly
                            property.SetValue(entity, value, null);
                        }
                    }
                    return entity;
                }
        */
        /*

                public static PropertyInfo GetPropertyFromEntity<T>(string key)
                {
                    var type = typeof(T);
                    var property = type.GetProperties().FirstOrDefault(p => p.Name.Equals(key));
                    if (property == null)
                    {
                        property = type.GetProperties()
                                        .FirstOrDefault(p =>
                                            p.GetCustomAttributes(typeof(OdooMapAttribute), false)
                                                .Cast<OdooMapAttribute>().Count(e => e.OdooName.Equals(key)) == 1);
                    }
                    return property;
                }

                public static OdooType GetOpenErpType(PropertyInfo property)
                {
                    var attributes = (OdooMapAttribute[])property.GetCustomAttributes(typeof(OdooMapAttribute), false);
                    return attributes.Length > 0 ? attributes[0].OdooType : OdooType.Undefined;
                }
        */

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
