using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Configuration;
using System.Reflection;

using Adc.Odoo.Service.Infrastructure.Exceptions;
using Adc.Odoo.Service.Infrastructure.Factories;
using Adc.Odoo.Service.Infrastructure.Interfaces;
using Adc.Odoo.Service.Models;

using CookComputing.XmlRpc;

namespace Adc.Odoo.Service
{
    public class OdooService
    {
        private const string DbPath = "/xmlrpc/db";

        private const string LoginPath = "/xmlrpc/2/common";

        private const string DataPath = "/xmlrpc/2/object";

        private readonly OdooConnection _connection;
        private readonly OdooServiceContext _context;

        public OdooService(OdooConnection connection)
        {
            if (!ToggleAllowUnsafeHeaderParsing(true))
            {
                throw new Exception("NO HTTPS Support");
            }
            _connection = connection;
            _context = new OdooServiceContext();

            _context.OdooDatabase = XmlRpcProxyGen.Create<IOdooDatabase>();
            _context.OdooDatabase.Url = string.Format(@"{0}{1}", _connection.Url, DbPath);

            _context.OdooAuthentication = XmlRpcProxyGen.Create<IOdooLogin>();
            _context.OdooAuthentication.Url = string.Format(@"{0}{1}", _connection.Url, LoginPath);

            _context.OdooData = XmlRpcProxyGen.Create<IOdoo>();
            _context.OdooData.Url = string.Format(@"{0}{1}", _connection.Url, DataPath);

            _context.Database = connection.Database;
            _context.Username = connection.Username;
            _context.Password = connection.Password;
        }


        /// <summary>
        /// List All databases 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDatabases()
        {
            try
            {
                var list = (string[])_context.OdooDatabase.List();
                return list;
            }
            catch (XmlRpcFaultException ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Login with the credentials from the <see cref="OdooConnection"/>
        /// </summary>
        /// <returns>Token if success or NULL if failed</returns>
        public int Login()
        {
            try
            {
                var value = _context.OdooAuthentication.Login(_connection.Database, _connection.Username, _connection.Password, null);
                int userid;
                if (int.TryParse(value.ToString(), out userid))
                {
                    return userid;
                }
                bool response;
                bool.TryParse(value.ToString(), out response);
                throw new OdooAuthenticationException("Login failed");
            }
            catch (XmlRpcFaultException ex)
            {
                throw new OdooAuthenticationException("Login failed, XmlRpc Error", ex);
            }
            catch (Exception ex)
            {

                throw new OdooAuthenticationException("Login failed, Error", ex);
            }
        }


        /// <summary>
        /// Code based fix for Https xml/rpc communication
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        private bool ToggleAllowUnsafeHeaderParsing(bool enable)
        {
            //Get the assembly that contains the internal class
            Assembly assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created already invoking the property will create it for us.
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }


        internal IEnumerable<T> GetEntities<T>(Expression<Func<T, bool>> conditions, int? offset = null, int? limit = null, string order = null) where T : IOdooObject, new()
        {
            try
            {
                OdooCommandContext context;
                context = OdooCommandContextFactory.BuildCommandContextFromExpression<T>(conditions);
                context.Limit = limit ?? 0;
                context.Offset = offset ?? 0;
                context.Order = order;
                IEnumerable<object> ids = SearchCommand(context);
                context.ClearArguments();
                ResultSet result = GetEntityCommand(context, ids);
                IEnumerable<T> entities = OdooObjectFactory.BuildEntities<T>(this, result);
                return entities;
            }
            catch (Exception ex)
            {
                return null;
            }

        }


        public IEnumerable GetEntities(OdooCommandContext context)
        {
            ResultSet resultset;
            IEnumerable<object> ids;
            ids = SearchCommand(context);
            context.ClearArguments();
            resultset = GetEntityCommand(context, ids);
            MethodInfo method = typeof(OdooObjectFactory).GetMethod("BuildEntities", new[] { this.GetType(), typeof(ResultSet) });
            method = method.MakeGenericMethod(context.EntityType);
            IEnumerable res = method.Invoke(this, new object[] { this, resultset }) as IEnumerable;
            return res.Cast<object>();
        }

        /// <summary>
        /// Gets an entity from openerp. Reads openerpmap attribute for underlaying property type and
        /// gets from openerp.
        /// </summary>
        /// <param name="property">Property info</param>
        /// <param name="id">Id of entity to search</param>
        /// <returns>Entity object</returns>
        internal object GetEntityById(OdooCommandContext context, int id)
        {
            ResultSet resultset;
            resultset = GetEntityCommand(context, new object[] { id });
            //Call a BuildEntities genereci method from this class
            MethodInfo method = typeof(OdooObjectFactory).GetMethod("BuildEntities", new[] { this.GetType(), typeof(ResultSet) });
            method = method.MakeGenericMethod(context.EntityType);
            IEnumerable res = method.Invoke(this, new object[] { this, resultset }) as IEnumerable;
            return res.Cast<object>().First();
        }

        /// <summary>
        /// Add entity to OpenErp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        internal int AddEntity<T>(T entity) where T : IOdooObject
        {
            var context = OdooCommandContextFactory.BuildCommandContextFromEntity<T>(entity);
            var id = AddCommand(context);
            entity.Id = id;
            return id;
        }

        /// <summary>
        /// Delete entity from OpenErp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        internal void DeleteEntity<T>(T entity) where T : IOdooObject
        {
            var context = OdooCommandContextFactory.BuildCommandContextFromEntity<T>(entity);
            DeleteCommand(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityName"></param>
        internal void DeleteEntityById(int id, string entityName)
        {
            DeleteCommand(id, entityName);
        }

        /// <summary>
        /// Update OpenErp entity with current entity values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        internal int UpdateEntity<T>(T entity) where T : IOdooObject
        {
            OdooCommandContext context = OdooCommandContextFactory.BuildCommandContextFromEntity<T>(entity);
            UpdateCommand(context);
            return entity.Id;
        }

        private IEnumerable<object> SearchCommand(OdooCommandContext commandContext)
        {
            _context.UserId = Login();
            try
            {
                if (commandContext.Limit != 0)
                {
                    return _context.OdooData.Search(_context.Database, _context.UserId, _context.Password, commandContext.EntityName, "search", commandContext.GetArguments(), commandContext.Offset, commandContext.Limit, commandContext.Order);
                }
                return _context.OdooData.Search(_context.Database, _context.UserId, _context.Password, commandContext.EntityName, "search", commandContext.GetArguments());
            }
            catch (XmlRpcFaultException e)
            {
                throw OdooException.GetException(e);
            }
        }

        /// <summary>
        /// Execute Add command in OpenErp with context params.
        /// 
        /// Exclude id property because OpenErp will return it after creation.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private int AddCommand(OdooCommandContext context)
        {
            int res;
            _context.UserId = Login();

            XmlRpcStruct fields = new XmlRpcStruct();
            foreach (OdooCommandArgument argument in context.Arguments)
            {
                if (!argument.ReadOnly)
                {
                    if (!argument.Property.Equals("id"))
                    {
                        fields.Add(argument.Property, argument.Value);
                    }
                }
            }
            try
            {
                res = _context.OdooData.Create(_context.Database, _context.UserId, _context.Password, context.EntityName, "create", fields);
            }
            catch (XmlRpcFaultException e)
            {
                throw OdooException.GetException(e);
            }
            return res;
        }

        /// <summary>
        /// Executes delete command in OpenErp. Get id from context.
        /// </summary>
        /// <param name="context"></param>
        private void DeleteCommand(OdooCommandContext context)
        {
            _context.UserId = Login();

            OdooCommandArgument idArgument = context.Arguments.FirstOrDefault(x => x.Property.Equals("id"));

            if (idArgument != null)
            {
                DeleteCommand((int)idArgument.Value, context.EntityName);
            }
        }

        private void DeleteCommand(int id, string entityName)
        {
            var idArray = new object[1];
            idArray[0] = id;
            try
            {
                _context.OdooData.Unlink(_context.Database, _context.UserId, _context.Password, entityName, "unlink", idArray);
            }
            catch (XmlRpcFaultException e)
            {
                throw OdooException.GetException(e);
            }

        }

        private void UpdateCommand(OdooCommandContext context)
        {
            _context.UserId = Login();
            var idArray = new int[1];
            //Transformamos los argumentos en XmlRpcStruct
            var fields = new XmlRpcStruct();
            foreach (var argument in context.Arguments)
            {
                if (!argument.Property.Equals("id"))
                {
                    if (argument.Value != null)
                    {
                        fields.Add(argument.Property, argument.Value);
                    }
                }
                else
                {
                    if (argument.Value != null)
                    {
                        idArray[0] = (int)argument.Value;
                    }
                }
            }

            try
            {
                _context.OdooData.Write(_context.Database, _context.UserId, _context.Password, context.EntityName, "write", idArray, fields);
            }
            catch (XmlRpcFaultException e)
            {
                throw OdooException.GetException(e);
            }
        }

        public ResultSet GetEntityCommand(OdooCommandContext commandContext, IEnumerable<object> ids)
        {
            _context.UserId = Login();

            try
            {
                var resultset = _context.OdooData.Read(_context.Database, _context.UserId, _context.Password, commandContext.EntityName, "read", ids.ToArray(), commandContext.GetArguments());
                return new ResultSet(resultset);
            }
            catch (XmlRpcFaultException e)
            {
                throw OdooException.GetException(e);
            }

        }


        /*private void ProcessSubObjects<T>(T item) where T : IOdooObject
        {
            var objects = item.GetType()
                .GetProperties()
                .Where(
                    p => p.GetCustomAttributes(typeof(OdooObjectAttribute), true)
                             .Any());

            foreach (var @object in objects)
            {
                var info = @object.GetCustomAttribute<OdooObjectAttribute>();
                if (@object.GetValue(item) != null)
                {

                    if (@object.GetValue(item).GetType().GetInterfaces().Contains(typeof(IOdooObject)))
                    {
                        switch (info.LinkType)
                        {
                            case OdooObjectLinkType.Many2One:
                                var prop = item.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(OdooPropertyAttribute), true).Any()).FirstOrDefault(x => x.Name == info.IdProp);
                                if (prop != null)
                                {
                                    var savedObject = AddOrUpdate((IOdooObject)@object.GetValue(item));
                                    prop.SetValue(item, savedObject.Id);
                                    @object.SetValue(item, savedObject);
                                }
                                break;
                        }
                    }
                }
            }
        }*/

    }
}
