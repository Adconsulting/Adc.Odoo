using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Adc.Odoo.Service.Infrastructure.Factories;
using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Service.Models
{
    public class OdooObject<T> : IOdooConverter<T> where T : IOdooObject, new()
    {
        private readonly int _id;

        private readonly OdooService _service;

        private readonly bool _dataLoaded;

        private T @object;



        public OdooObject(OdooService service)
        {
            _service = service;
            _dataLoaded = false;
        }

        public OdooObject(OdooService service, int id)
            : this(service)
        {
            _id = id;
        }

        private void LoadData()
        {
            if (!_dataLoaded)
            {
                var context = new OdooCommandContext();
                context.EntityName = OdooCommandContextFactory.GetOdooEntityName(typeof(T));

                var result = _service.GetEntityCommand(context, new List<object> { _id });

                var collection = new Collection<T>();

                OdooObjectFactory.BuildEntities(_service, result, collection);

                if (collection.Any())
                {
                    @object = collection.First();
                }
            }
        }

        public T GetObject()
        {
            LoadData();
            return @object;
        }
    }
}
