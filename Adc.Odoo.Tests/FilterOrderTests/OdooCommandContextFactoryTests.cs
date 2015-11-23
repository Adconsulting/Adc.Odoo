using System;
using System.Linq;

using Adc.Odoo.Service.Infrastructure.Factories;
using Adc.Odoo.Tests.Models;

using NUnit.Framework;

namespace Adc.Odoo.Tests.FilterOrderTests
{
    [TestFixture]
    public class OdooCommandContextFactoryTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SinglePropStringSearchTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(
                x => x.Name == "test");

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());
            Assert.That(command.Arguments.Count == 1);
            Assert.That(command.Arguments.First().Operation == "=");
        }

        [Test]
        public void DoublePropAndStringSearchTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(
                x => x.Name == "test" && x.Email.Contains("@"));

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());

            var param = command.GetArguments();

            Assert.That(param.Any());
            Assert.That(param.Count() == 2);

            var arg1 = (object[])param[0];
            var arg2 = (object[])param[1];

            Assert.That((string)arg1[1] == "=");
            Assert.That((string)arg2[1] == "ilike");

        }

        [Test]
        public void TriplePropAndStringSearchTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(
                x => x.Name == "test" && x.Email.Contains("@") && x.Email.StartsWith("test"));

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());

            var param = command.GetArguments();

            Assert.That(param.Any());
            Assert.That(param.Count() == 3);

            var arg1 = (object[])param[0];
            var arg2 = (object[])param[1];
            var arg3 = (object[])param[2];

            Assert.That((string)arg1[1] == "=");
            Assert.That((string)arg2[1] == "ilike");
            Assert.That((string)arg3[1] == "like");

        }

        [Test]
        public void SinglePropBoolTrueSearchTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(x =>
                x.IsCompany);

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());

            var param = command.GetArguments();

            Assert.That(param.Any());
            Assert.That(param.Count() == 1);

            var arg1 = (object[])param[0];

            Assert.That((string)arg1[1] == "=");
            Assert.That((bool)arg1[2] == true);


        }

        [Test]
        public void SinglePropBoolFalseSearchTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(x =>
                !x.IsCompany);

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());

            var param = command.GetArguments();

            Assert.That(param.Any());
            Assert.That(param.Count() == 1);

            var arg1 = (object[])param[0];

            Assert.That((string)arg1[1] == "=");
            Assert.That((bool)arg1[2] == false);
        }


        [Test]
        public void OrTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(x =>
                x.Name == "test" || x.Name == "info");

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());

            var args = command.GetArguments();

            Assert.That(args.Count() == 3);
        }


        [Test]
        public void MultiOrTest()
        {
            var command = OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(x =>
                x.Name == "test" || x.Name == "info" || x.Name == "test2");

            Assert.IsNotNull(command);
            Assert.That(command.Arguments.Any());

            var args = command.GetArguments();

            Assert.That(args.Count() == 4);
        }

        [Test]
        public void AndOrTest()
        {
           Assert.Throws<NotImplementedException>(() => OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(x =>
                x.Email == "test" && (x.Name == "test" || x.Name == "info")));

           Assert.Throws<NotImplementedException>(() => OdooCommandContextFactory.BuildCommandContextFromExpression<OdooPartner>(x =>
           x.Email == "test" && x.Name == "test" || x.Name == "info"));
            
        }



        [TearDown]
        public void TearDown()
        {

          

        }
    }
}