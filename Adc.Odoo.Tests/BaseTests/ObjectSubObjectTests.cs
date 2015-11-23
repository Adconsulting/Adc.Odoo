using System.Linq;

using Adc.Odoo.Service;
using Adc.Odoo.Service.Infrastructure.Extensions;
using Adc.Odoo.Service.Models;
using Adc.Odoo.Tests.Models;

using NUnit.Framework;

namespace Adc.Odoo.Tests.BaseTests
{
    [TestFixture]
    public class ObjectSubObjectTests : OdooTestBase
    {
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void ObjectWithlistTest()
        {
            var service = new OdooService(Connection);

            var project = service.FirstOrDefault<OdooProject>(OdooFilter<OdooProject>.Where(x => x.Id == 431));

            var first  = project.PartnerInvoices.First();

            Assert.That(project.PartnerInvoices.Any());
        }



        [TearDown]
        public void TearDown()
        {
        }
    }
}