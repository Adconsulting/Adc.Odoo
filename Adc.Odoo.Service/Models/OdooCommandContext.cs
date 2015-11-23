using System;
using System.Collections.Generic;
using System.Linq;

namespace Adc.Odoo.Service.Models
{
    public class OdooCommandContext
    {
        private List<OdooCommandArgument> _arguments;

        public string EntityName { get; set; }
        public Type EntityType { get; set; }
        public string ParameterName { get; set; }

        public List<OdooCommandArgument> Arguments
        {
            get
            {
                return _arguments;
            }
            set
            {
                _arguments = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Offset
        /// </summary>
        public virtual int Offset { get; set; }

        /// <summary>
        /// Gets or Sets the Limit
        /// </summary>
        public virtual int Limit { get; set; }

        /// <summary>
        /// Gets or Sets the Order
        /// </summary>
        public virtual string Order { get; set; }

        public OdooCommandContext()
        {
            this.Arguments = new List<OdooCommandArgument>();
        }

        public object[] GetArguments()
        {
            var objectList = new List<object>();
           
            foreach (OdooCommandArgument argument in _arguments.Where(x => x.CompareType != "|").OrderBy(x => x.Order))
            {
                objectList.Add(new[] { argument.Property, argument.Operation, argument.Value });
            }

            if (_arguments.Any(x => x.CompareType == "|"))
            {
                var list = new List<object>();
                list.Add("|");
                list.AddRange(objectList);
                
                return list.ToArray() ;
                //return new []{list.ToArray()};

                //objectList.Add(new[] { _arguments.First(x => x.CompareType == "|").CompareType });
            }

            return objectList.ToArray();
        }

        public void ClearArguments()
        {
            _arguments.Clear();
        }
    }
}
