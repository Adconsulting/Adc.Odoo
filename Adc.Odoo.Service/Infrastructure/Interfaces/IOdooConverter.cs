using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adc.Odoo.Service.Infrastructure.Interfaces
{
    public interface IOdooConverter<out T> where T : IOdooObject, new()
    {
        T GetObject();

    }
}
