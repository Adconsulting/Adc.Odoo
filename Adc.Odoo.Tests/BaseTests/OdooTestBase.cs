using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Adc.Odoo.Service.Models;

namespace Adc.Odoo.Tests.BaseTests
{
    public class OdooTestBase
    {
        protected readonly OdooConnection Connection = new OdooConnection
                                                  {
                                                      Url = "",
                                                      Username = "",
                                                      Password = "",
                                                      Database = "",
                                                  };

    }
}
