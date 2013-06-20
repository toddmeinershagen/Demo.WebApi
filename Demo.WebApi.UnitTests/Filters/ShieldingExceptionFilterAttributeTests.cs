using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Common.Logging.Simple;
using Demo.WebApi.Filters;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Demo.WebApi.UnitTests.Filters
{
    [TestFixture]
    public class ShieldingExceptionFilterAttributeTests
    {
        private CapturingLoggerFactoryAdapter _adapter;
        private ShieldingExceptionFilterAttribute _filter;
        private const string Message = "An unknown error has occured.";
        private AssertionException _exception;
        private Guid _id;
        private HttpActionExecutedContext _context;

        [SetUp]
        public void SetUp ()
        {
            _adapter = new CapturingLoggerFactoryAdapter();
            Common.Logging.LogManager.Adapter = _adapter;

            _filter = new ShieldingExceptionFilterAttribute();
            _id = Guid.NewGuid();
            _filter.GenerateNewGuid = () => _id;
            _context = ContextUtil.GetActionExecutedContext(Substitute.For<HttpRequestMessage>(), Substitute.For<HttpResponseMessage>());

            _exception = new AssertionException(Message);

            _context.Exception = _exception;
            _filter.OnException(_context);
        }

        [Test]
        public void given_exception_occurs_when_filtering_the_exception_should_log_the_exception_details()
        {
            _adapter.LastEvent.Exception.Should().Be(_exception);
            _adapter.LastEvent.RenderedMessage.Should().Be(Message);
        }

        [Test]
        public void given_exception_occurs_when_filtering_the_exception_should_log_as_error()
        {
            _adapter.LastEvent.Level.Should().Be(Common.Logging.LogLevel.Error);
        }

        [Test]
        public void given_exception_occurs_when_filtering_the_exception_should_add_identifier_to_exception()
        {
            _adapter.LastEvent.Exception.Data["Id"].Should().Be(_id);
        }

        [Test]
        public void given_exception_occurs_when_filtering_the_exception_should_return_status_of_internal_server_error()
        {
            _context.Response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public void given_exception_occurs_when_filtering_the_exception_should_return_generic_error_message()
        {
            var expectedMessage = string.Format("An error occurred on the server (Error Id:  {0}).  If you continue to experience this problem, contact your administrator.", _id);
            var content = _context.Response.Content as ObjectContent<HttpError>;
            var value = content.Value as HttpError;
            value["Message"].Should().Be(expectedMessage);
        }

    }

    internal static class ContextUtil
	     {
	         public static HttpControllerContext CreateControllerContext(HttpConfiguration configuration = null, IHttpController instance = null, IHttpRouteData routeData = null, HttpRequestMessage request = null)
	         {
	             HttpConfiguration config = configuration ?? new HttpConfiguration();
             IHttpRouteData route = routeData ?? new HttpRouteData(new HttpRoute());
	             HttpRequestMessage req = request ?? new HttpRequestMessage();
	             req.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
	             req.Properties[HttpPropertyKeys.HttpRouteDataKey] = route;
	  
	             HttpControllerContext context = new HttpControllerContext(config, route, req);
	             if (instance != null)
	             {
	                 context.Controller = instance;
	             }
	             context.ControllerDescriptor = CreateControllerDescriptor(config);
	  
	             return context;
	         }
	  
	         public static HttpActionContext CreateActionContext(HttpControllerContext controllerContext = null, HttpActionDescriptor actionDescriptor = null)
	         {
	             HttpControllerContext context = controllerContext ?? CreateControllerContext();
	             HttpActionDescriptor descriptor = actionDescriptor ?? CreateActionDescriptor();
	             descriptor.ControllerDescriptor = context.ControllerDescriptor;
	             return new HttpActionContext(context, descriptor);
	         }
	  
	         public static HttpActionContext GetHttpActionContext(HttpRequestMessage request)
	         {
	             HttpActionContext actionContext = CreateActionContext();
	             actionContext.ControllerContext.Request = request;
	             return actionContext;
	         }
	  
	         public static HttpActionExecutedContext GetActionExecutedContext(HttpRequestMessage request, HttpResponseMessage response)
	         {
	             HttpActionContext actionContext = CreateActionContext();
	             actionContext.ControllerContext.Request = request;
	             HttpActionExecutedContext actionExecutedContext = new HttpActionExecutedContext(actionContext, null) { Response = response };
	             return actionExecutedContext;
	         }
	  
          public static HttpControllerDescriptor CreateControllerDescriptor(HttpConfiguration config = null)
	         {
	             if (config == null)
 
                 {
	                 config = new HttpConfiguration();
	             }
	             return new HttpControllerDescriptor() { Configuration = config, ControllerName = "FooController" };
	         }
	  
	         public static HttpActionDescriptor CreateActionDescriptor()
	         {
	             var mock = Substitute.For<HttpActionDescriptor>();
	             mock.ActionName.Returns("Bar");
	             return mock;
	         }
	     }
}
