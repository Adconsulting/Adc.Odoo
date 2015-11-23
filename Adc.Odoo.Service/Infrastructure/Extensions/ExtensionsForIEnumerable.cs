using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Adc.Odoo.Service.Infrastructure.Attributes;
using Adc.Odoo.Service.Infrastructure.Factories;
using Adc.Odoo.Service.Infrastructure.Interfaces;
using Adc.Odoo.Service.Models;

namespace Adc.Odoo.Service.Infrastructure.Extensions
{
    public static class ExtensionsForIEnumerable
    {
        /// <summary>
        /// If property passed in path argument is a OpenErp class, 
        /// it will load from OpenErp and set path property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="source"></param>
        /// <param name="path">Property to load</param>
        /// <returns></returns>
        public static IEnumerable<T> Include<T, TProperty>(this IEnumerable<T> source, Expression<Func<T, TProperty>> path) where T : IOdooObject, new()
        {
            var enumerable = source as OdooCollection<T>;
            if (enumerable != null)
            {
                var service = enumerable.Service;
                var memberAccess = path.Body as MemberExpression;
                if (memberAccess != null)
                {
                    var member = memberAccess.Member;
                    if (member != null)
                    {
                        //Search for openerpmap attribute
                        Type memberType = ((PropertyInfo)memberAccess.Member).PropertyType;
                        OdooMapAttribute[] attributes;
                        if (memberType != null)
                        {
                            if (memberType.IsGenericCollection())
                            {
                                //Collection, load entities
                                //If collection is type of OpenErpSet, just load data.
                                //Else, try to get openerpattribute
                                Type EnumerationType = memberType.GetGenericArguments()[0];
                                attributes = (OdooMapAttribute[])EnumerationType.GetCustomAttributes(typeof(OdooMapAttribute), false);
                                var accessFunction = path.Compile();
                                foreach (T item in enumerable)
                                {
                                    var value = accessFunction(item);
                                    if (value != null)
                                    {
                                        //Check for OpenErpSet and call load.
                                        Type openErpSetType = typeof(OdooCollection<>).MakeGenericType(EnumerationType);
                                        if (value.GetType() == openErpSetType)
                                        {
                                            //call load data
                                            var res = value.GetType().GetMethod("LoadData").Invoke(value, null);
                                        }
                                    }
                                    else
                                    {
                                        //if has openerpmap attribute, load related data, else nothing can be done
                                        if (EnumerationType != null)
                                        {
                                            if (attributes.Length > 0)
                                            {
                                                //property type has a OpenErp mapped attribute
                                                PropertyInfo idProperty = member.DeclaringType.GetProperties().Where(p => p.Name.Equals("Id")).Single();
                                                int id = (int)idProperty.GetValue(item, null);
                                                OdooCommandContext context = new OdooCommandContext();
                                                context.EntityName = attributes[0].OdooName;
                                                context.EntityType = EnumerationType;
                                                string fieldName = ((OdooMapAttribute)(member.GetCustomAttributes(false).First())).OdooName;
                                                context.Arguments.Add(new OdooCommandArgument() { Operation = "=", Property = fieldName, Value = id });
                                                Object res = service.GetEntities(context);
                                                ((PropertyInfo)memberAccess.Member).SetValue(item, res, null);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Single entity to load.
                                //Check basic type
                                if (memberType.IsPrimitive || memberType.Equals(typeof(string)))
                                {
                                    // Nothing can be done
                                }
                                else
                                {
                                    // Have an entity
                                    OdooForeignKeyAttribute fk;
                                    OdooCommandContext context = new OdooCommandContext();
                                    context.EntityName = OdooCommandContextFactory.GetOdooEntityName(memberType);
                                    context.EntityType = memberType;
                                    fk = (OdooForeignKeyAttribute)member.GetCustomAttributes(typeof(OdooForeignKeyAttribute), false).FirstOrDefault();
                                    if (fk != null)
                                    {
                                        PropertyInfo idProperty = member.DeclaringType.GetProperties().Where(p => p.Name.Equals(fk.PropertyName)).Single();
                                        foreach (T item in enumerable)
                                        {
                                            int id = (int)idProperty.GetValue(item, null);
                                            //Buscamos en OpenErp
                                            Object res = service.GetEntityById(context, id);
                                            ((PropertyInfo)memberAccess.Member).SetValue(item, res, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Not a valid property to include.");
                }
            }
            return source;
        }
    }
}
