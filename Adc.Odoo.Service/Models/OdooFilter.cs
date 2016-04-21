using System;
using System.Linq.Expressions;

using Adc.Odoo.Service.Infrastructure.Interfaces;

namespace Adc.Odoo.Service.Models
{
    public class OdooFilter<T> where T : IOdooObject
    {
        private readonly Expression<Func<T, bool>> _filter;

        public OdooFilter()
        {

        }

        public OdooFilter(Expression<Func<T, bool>> filter)
        {
            _filter = filter;
        }

        /// <summary>
        /// Gets or Sets the Filter
        /// </summary> 
        public Expression<Func<T, bool>> Filter
        {
            get
            {
                return _filter;
            }
        }

        public static OdooFilter<T> Where(Expression<Func<T, bool>> filter)
        {
            return new OdooFilter<T>(filter);
        }
    }
}
