using System.Collections.Generic;
using System.Linq;

using Adc.Odoo.Service.Infrastructure.Factories;
using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Service.Models
{
    public class OdooCollection<T> : ICollection<T> where T : IOdooObject, new()
    {

        public ICollection<T> Entities { get; set; }
        public object[] EntitiesId { get; set; }
        public bool DataLoaded { get; set; }
        public OdooService Service { get; set; }

        public OdooCollection(OdooService service)
        {
            this.Service = service;
            this.DataLoaded = false;
            this.Entities = new List<T>();
        }

        public OdooCollection(OdooService service, object[] ids)
            : this(service)
        {
            this.EntitiesId = ids;
        }

        public OdooCollection(OdooService service, int[] ids)
            : this(service)
        {
            this.EntitiesId = ids.Cast<object>().ToArray();
        }

        public void LoadData()
        {
            if (!DataLoaded)
            {
                if (EntitiesId != null && EntitiesId.Length > 0)
                {
                    this.DataLoaded = true;
                    //Call for get entities by ids.
                    OdooCommandContext context = new OdooCommandContext();
                    context.EntityName = OdooCommandContextFactory.GetOdooEntityName(typeof(T));
                    ResultSet result = this.Service.GetEntityCommand(context, this.EntitiesId);
                    OdooObjectFactory.BuildEntities<T>(Service, result, this);
                }
            }
        }

        public void Add(T item)
        {
            this.Entities.Add(item);
        }

        public void Clear()
        {
            this.Entities.Clear();
        }

        public bool Contains(T item)
        {
            this.LoadData();
            return this.Entities.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.LoadData();
            this.Entities.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                LoadData();
                return Entities.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            this.LoadData();
            return this.Entities.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            this.LoadData();
            return this.Entities.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            this.LoadData();
            return this.Entities.GetEnumerator();
        }
    }
}
