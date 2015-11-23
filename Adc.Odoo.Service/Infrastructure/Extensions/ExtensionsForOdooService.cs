using System;
using System.Collections.Generic;
using System.Linq;

using Adc.Odoo.Service.Infrastructure.Interfaces;
using Adc.Odoo.Service.Models;

namespace Adc.Odoo.Service.Infrastructure.Extensions
{
    public static class ExtensionsForOdooService
    {
        public static T First<T>(this OdooService service,  OdooFilter<T> filter) where T : IOdooObject, new()
        {
            var result = service.GetEntities(filter.Filter).ToList();
            if (result.Any()) return result.First();
            throw new ArgumentNullException();
        }

        public static T FirstOrDefault<T>(this OdooService service, OdooFilter<T> filter) where T : IOdooObject, new()
        {
            var result = service.GetEntities(filter.Filter).ToList();
            return result.FirstOrDefault();
        }

        public static ICollection<T> List<T>(this OdooService service, OdooFilter<T> filter, OdooSorter<T> sorter = null, int? offset = null, int? limit = null )
            where T : IOdooObject, new()
        {
            if (sorter != null & (offset == null || limit == null))
                throw new ArgumentNullException("sorter", "A sorter requires offset and limit");

            if ((offset != null && limit == null) || (offset == null && limit != null))
                throw new ArgumentNullException("offset", "page and offset are required");

            if (sorter != null)
                return service.GetEntities(filter.Filter, offset, limit, sorter.Order)
                    .ToList();

            return  service.GetEntities(filter.Filter).ToList();
        }

        public static int AddOrUpdate<T>(this OdooService service, T item) where T : IOdooObject
        {
            if (item.Id == 0)
            {
                return service.AddEntity(item);
            }
            else
            {
                return service.UpdateEntity(item);
            }
            
        }

        public static void Delete<T>(this OdooService service, T item) where T : IOdooObject
        {
            service.DeleteEntity(item);
        }
    }
}
