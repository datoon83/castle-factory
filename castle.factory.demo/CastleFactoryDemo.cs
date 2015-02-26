using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NUnit.Framework;
using System.Reflection;

namespace castle.factory.demo
{
    public class CastleFactoryDemo
    {
        private IWindsorContainer _container;

        [SetUp]
        public void Setup()
        {
            _container = new WindsorContainer();
            _container.Install(new ApplicationInstaller());
        }

        [Test]
        public void IShouldReceive_ACancelledOrderEmailBodyBuilder_WhenIPassInACancelledOrderEvent()
        {
            var factory = _container.Resolve<IEmailBodyFactory>();

            var emailBuilder = factory.GetByEmailType(new CancelledOrderEvent());
            var message = emailBuilder.Build();

            Assert.That(message, Is.EqualTo("cancelled order"));
        }

        [Test]
        public void IShouldReceive_ANewOrderEmailBodyBuilder_WhenIPassInANewOrderEvent()
        {
            var factory = _container.Resolve<IEmailBodyFactory>();

            var emailBuilder = factory.GetByEmailType(new NewOrderEvent());
            var message = emailBuilder.Build();

            Assert.That(message, Is.EqualTo("order body"));
        }

        [TearDown]
        public void Teardown()
        {
            _container.Dispose();
        }
    }

    public class ApplicationInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<TypedFactoryFacility>();

            container.Register(
                Component.For<IEmailBodyFactory>()
                    .AsFactory(c => c.SelectedWith(new CustomTypedFactoryComponentSelector())));

            container.Register(
                Component.For<IBuildEmailBodies>()
                    .ImplementedBy<CancelledOrderBuilder>()
                    .Named("CancelledOrderBuilder"),

                Component.For<IBuildEmailBodies>()
                    .ImplementedBy<NewOrderBuilder>()
                    .Named("NewOrderBuilder"));
        }
    }

    public class CustomTypedFactoryComponentSelector : DefaultTypedFactoryComponentSelector
    {
        protected override string GetComponentName(MethodInfo method, object[] arguments)
        {
            if (method.Name == "GetByEmailType" && arguments.Length == 1)
            {
                return arguments[0].GetType().Name.Replace("Event", "Builder");
            }
            return base.GetComponentName(method, arguments);
        }
    }

    public interface IEmailBodyFactory
    {
        IBuildEmailBodies GetByEmailType<T>(T typeOfInterface);
    }

    public interface IBuildEmailBodies
    {
        string Build();
    }

    public class CancelledOrderBuilder : IBuildEmailBodies
    {
        public string Build()
        {
            return "cancelled order";
        }
    }

    public class NewOrderBuilder : IBuildEmailBodies
    {
        public string Build()
        {
            return "order body";
        }
    }

    public class CancelledOrderEvent
    {
    }

    public class NewOrderEvent
    {
    }
}
