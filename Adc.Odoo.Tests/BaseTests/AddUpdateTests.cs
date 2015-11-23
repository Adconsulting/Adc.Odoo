using Adc.Odoo.Service;
using Adc.Odoo.Service.Infrastructure.Extensions;
using Adc.Odoo.Service.Models;
using Adc.Odoo.Tests.Models;

using NUnit.Framework;

namespace Adc.Odoo.Tests.BaseTests
{
    [TestFixture]
    public class AddUpdateTests : OdooTestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AddTest()
        {
            var service = new OdooService(Connection);

            var partner = new OdooPartner { Email = "test@email.com", IsCompany = true, Name = "Test Company" };

            var id  = service.AddOrUpdate(partner);
            
            Assert.That(id > 0);

            var check = service.First(OdooFilter<OdooPartner>.Where(x => x.Id == id));

            Assert.That(check.Name == "Test Company");
        }

        [Test]
        public void UpdateTest()
        {
            var service = new OdooService(Connection);

            var partner = new OdooPartner { Email = "test@email.com", IsCompany = true, Name = "Test Company" };

            var id = service.AddOrUpdate(partner);

            Assert.That(id > 0);

            var check = service.First(OdooFilter<OdooPartner>.Where(x => x.Id == id));

            Assert.That(check.Name == "Test Company");
            partner.Name = "NEW NAME";


            service.AddOrUpdate(partner);

            var check2 = service.First(OdooFilter<OdooPartner>.Where(x => x.Id == id));

            Assert.That(check2.Name == "NEW NAME");
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}