using System;
using System.Linq;

using Adc.Odoo.Service;
using Adc.Odoo.Service.Infrastructure.Extensions;
using Adc.Odoo.Service.Models;
using Adc.Odoo.Tests.Models;

using NUnit.Framework;

namespace Adc.Odoo.Tests.BaseTests
{
    [TestFixture]
    public class GetObjectsFromTests : OdooTestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FirstItemsFoundTest()
        {
            var service = new OdooService(Connection);

            var partner = service.First(OdooFilter<OdooPartner>.Where(x => x.Id == 3354));

            Assert.IsNotNull(partner);
            Assert.That(partner.Id == 3354);
        }

        [Test]
        public void FirstItemsNotFoundTest()
        {
            var service = new OdooService(Connection);

            Assert.Throws<ArgumentNullException>(() => service.First(OdooFilter<OdooPartner>.Where(x => x.Id == 0)));
        }

        [Test]
        public void FirstOrDefaultItemsFoundTest()
        {
            var service = new OdooService(Connection);

            var partner = service.FirstOrDefault(OdooFilter<OdooPartner>.Where(x => x.Id == 3354));

            Assert.IsNotNull(partner);
            Assert.That(partner.Id == 3354);

        }

        [Test]
        public void FirstOrDefaultItemsNotFoundTest()
        {
            var service = new OdooService(Connection);

            var partner = service.FirstOrDefault(OdooFilter<OdooPartner>.Where(x => x.Id == 0));

            Assert.IsNull(partner);
        }

        [Test]
        public void BasicListTest()
        {

            var service = new OdooService(Connection);

            var results = service.List<OdooPartner>(OdooFilter<OdooPartner>.Where(x=> true), OdooSorter<OdooPartner>.OrderBy(x=> x.Id), 0, 4);

            Assert.IsNotNull(results);
            Assert.That(results.Any());
            Assert.That(results.Count == 4);
        }

        [Test]
        public void ListPagingOrderByTest()
        {

            var service = new OdooService(Connection);

            var results = service.List<OdooPartner>(OdooFilter<OdooPartner>.Where(x => true), OdooSorter<OdooPartner>.OrderBy(x=> x.Id), 0, 2);
            var results2 = service.List<OdooPartner>(OdooFilter<OdooPartner>.Where(x => true), OdooSorter<OdooPartner>.OrderBy(x => x.Id), 2, 2);

            Assert.IsNotNull(results);
            Assert.That(results.Any());
            Assert.That(results.Count == 2);


            Assert.IsNotNull(results2);
            Assert.That(results2.Any());
            Assert.That(results2.Count == 2);

        }

        [Test]
        public void ListPagingOrderByDescTest()
        {

            var service = new OdooService(Connection);

            var results = service.List<OdooPartner>(OdooFilter<OdooPartner>.Where(x => true), OdooSorter<OdooPartner>.OrderByDescending(x => x.Id), 0, 2);
            var results2 = service.List<OdooPartner>(OdooFilter<OdooPartner>.Where(x => true), OdooSorter<OdooPartner>.OrderByDescending(x => x.Id), 2, 2);

            Assert.IsNotNull(results);
            Assert.That(results.Any());
            Assert.That(results.Count == 2);
            Assert.That(results.First().Id > results2.First().Id);

            Assert.IsNotNull(results2);
            Assert.That(results2.Any());
            Assert.That(results2.Count == 2);

        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}