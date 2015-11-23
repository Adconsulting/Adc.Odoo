using System;
using System.Linq.Expressions;

using Adc.Odoo.Service.Infrastructure.Attributes;

namespace Adc.Odoo.Service.Models
{
    public class OdooSorter<T>
    {
        private readonly Expression<Func<T, object>> _sorter;

        private readonly string _direction;


        public OdooSorter(Expression<Func<T, object>> sorter, string direction)
        {
            _sorter = sorter;
            _direction = direction;
        }

        public static OdooSorter<T> OrderBy(Expression<Func<T, object>> sorter)
        {
            return new OdooSorter<T>(sorter, "ASC");

        }

        public static OdooSorter<T> OrderByDescending(Expression<Func<T, object>> sorter)
        {
            return new OdooSorter<T>(sorter, "DESC");

        }

        public string Order
        {
            get
            {
                var exp = _sorter.Body;

                if (exp.NodeType == ExpressionType.MemberAccess)
                {
                    var param = exp as MemberExpression;
                    if (param != null)
                    {
                        var attribute = (OdooMapAttribute[])param.Member.GetCustomAttributes(typeof(OdooMapAttribute), false);
                        if (attribute[0] != null)
                        {
                            return String.Format(@"{0} {1}", attribute[0].OdooName, _direction);
                        }
                    }
                    return string.Empty;
                }
                return string.Empty;
            }
        }




    }
}
