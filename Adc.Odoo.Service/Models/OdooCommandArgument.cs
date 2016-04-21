using System;

using Adc.Odoo.Service.Infrastructure.Enums;

namespace Adc.Odoo.Service.Models
{
    public class OdooCommandArgument
    {
        public string Property { get; set; }
        public string Operation { get; set; }

        public string CompareType { get; set; }

        private object _value;

        public object Value
        {
            get
            {
                if (this.ArgumentType != OdooType.Undefined)
                {
                    switch (this.ArgumentType)
                    {
                        case OdooType.Undefined:
                            break;
                        case OdooType.Boolean:
                            break;
                        case OdooType.Integer:
                            break;
                        case OdooType.Float:
                            break;
                        case OdooType.Char:
                            if (this._value is DateTime)
                            {
                                return ((DateTime)this._value).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            break;
                        case OdooType.Text:
                            break;
                        case OdooType.Date:
                            if (this._value is DateTime)
                            {
                                return ((DateTime)this._value).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            break;
                        case OdooType.Datetime:
                            if (this._value is DateTime)
                            {
                                return ((DateTime)this._value).ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            break;
                        case OdooType.Binary:
                            break;
                        case OdooType.Selection:
                            break;
                        default:
                            break;
                    }
                }
                return this._value;
            }
            set { _value = value; }
        }

        public OdooType ArgumentType { get; set; }

        public int Order { get; set; }

        public bool ReadOnly { get; set; }
    }
}
