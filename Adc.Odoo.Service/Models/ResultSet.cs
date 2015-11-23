using System;

namespace Adc.Odoo.Service.Models
{
    public class ResultSet
    {
        public object[] Data { get; set; }

        public ResultSet(Object[] data)
        {
            this.Data = data;
        }
    }
}
