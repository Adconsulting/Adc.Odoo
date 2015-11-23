using Adc.Odoo.Service.Models;
using Adc.Odoo.Tests.Models;

using NUnit.Framework;

namespace Adc.Odoo.Tests.FilterOrderTests
{
    [TestFixture]
    public class OdooSorterTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void OrderByPropertyTest()
        {
            var sorter = OdooSorter<OdooPartner>.OrderBy(x => x.Name);
            Assert.That(sorter.Order.Contains("ASC"));
            Assert.That(sorter.Order.Contains("name"));

        }

        [Test]
        public void OrderByDescendingPropertyTest()
        {
            var sorter = OdooSorter<OdooPartner>.OrderByDescending(x => x.Name);
            Assert.That(sorter.Order.Contains("DESC"));
            Assert.That(sorter.Order.Contains("name"));
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}