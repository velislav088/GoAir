namespace GoAir.Web.Tests
{
    using System.Reflection;

    using GCommon;
    
    using Microsoft.AspNetCore.Authorization;
    
    [TestFixture]
    public class AuthorizationConventionsTests
    {
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AircraftController))]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AirportController))]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.FlightController))]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.HomeController))]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.TicketController))]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.ReviewController))]
        public void AdminAreaControllers_ShouldRequireAdministratorRole(Type controllerType)
        {
            AuthorizeAttribute? authorize = controllerType.GetCustomAttribute<AuthorizeAttribute>();
            Assert.That(authorize, Is.Not.Null);
            Assert.That(authorize!.Roles, Is.EqualTo(ApplicationRoles.Administrator));
        }
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AircraftController), "Create")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AircraftController), "Edit")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AircraftController), "Delete")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AirportController), "Create")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AirportController), "Edit")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.AirportController), "Delete")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.FlightController), "Create")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.FlightController), "Edit")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.FlightController), "Delete")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.TicketController), "Create")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.TicketController), "Edit")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.TicketController), "Delete")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.ReviewController), "Create")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.ReviewController), "Edit")]
        [TestCase(typeof(GoAir.Web.Areas.Administration.Controllers.ReviewController), "Delete")]
        public void AdminCatalogActions_ShouldRequireAdminRole(Type controllerType, string actionName)
        {
            MethodInfo[] methods = controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.Name == actionName)
            .ToArray();
            Assert.That(methods, Is.Not.Empty);
            Assert.That(methods.All(method =>
            {
                AuthorizeAttribute? authorize = method.GetCustomAttribute<AuthorizeAttribute>();
                return authorize == null || authorize.Roles == ApplicationRoles.Administrator;
            }), Is.True);
        }
        [Test]
        public void TicketController_ShouldRequireAuthenticatedUsers()
        {
            AuthorizeAttribute? authorize = typeof(GoAir.Web.Controllers.TicketController).GetCustomAttribute<AuthorizeAttribute>();
            Assert.That(authorize, Is.Not.Null);
        }
        [Test]
        public void ReviewController_ShouldRequireAuthenticatedUsers()
        {
            AuthorizeAttribute? authorize = typeof(GoAir.Web.Controllers.ReviewController).GetCustomAttribute<AuthorizeAttribute>();
            Assert.That(authorize, Is.Not.Null);
        }
    }
}