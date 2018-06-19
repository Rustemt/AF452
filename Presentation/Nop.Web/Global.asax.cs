using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using FluentValidation.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Web.Framework;
using Nop.Web.Framework.EmbeddedViews;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Routes;
using Nop.Services.Tasks;
using Nop.Web.Framework.Themes;
using Nop.Core.Domain;
using MvcMiniProfiler.MVCHelpers;
using System.Web.Configuration;
using MvcMiniProfiler;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;

namespace Nop.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
           // filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
            
            //register custom routes (plugins, etc)
            var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
            routePublisher.RegisterRoutes(routes);
            
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "Nop.Web.Controllers" }
            );
        }

        protected void Application_Start()
        {
            //initialize engine context
            EngineContext.Initialize(false);

            bool databaseInstalled = DataSettingsHelper.DatabaseIsInstalled();
            //start scheduled tasks
            if (databaseInstalled)
            {
                TaskManager.Instance.Initialize();
                TaskManager.Instance.Start();
            }

            //set dependency resolver
            var dependencyResolver = new NopDependencyResolver();
            DependencyResolver.SetResolver(dependencyResolver);

            //model binders
            ModelBinders.Binders.Add(typeof(BaseNopModel), new NopModelBinder());

            if (databaseInstalled)
            {
                //remove all view engines
                ViewEngines.Engines.Clear();
                //except the themeable razor view engine we use
                ViewEngines.Engines.Add(new ThemeableRazorViewEngine());
            }

            //Add some functionality on top of the default ModelMetadataProvider
            ModelMetadataProviders.Current = new NopMetadataProvider();

            //Registering some regular mvc stuf
            AreaRegistration.RegisterAllAreas();
            if (databaseInstalled &&
                EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                GlobalFilters.Filters.Add(new ProfilingActionFilter());
            }
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;

            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new NopValidatorFactory()));

            //register virtual path provider for embedded views
            var embeddedViewResolver = EngineContext.Current.Resolve<IEmbeddedViewResolver>();
            var embeddedProvider = new EmbeddedViewVirtualPathProvider(embeddedViewResolver.GetEmbeddedViews());
            HostingEnvironment.RegisterVirtualPathProvider(embeddedProvider);

            if (databaseInstalled)
            {
                if (EngineContext.Current.Resolve<StoreInformationSettings>().MobileDevicesSupported)
                {
                    //Enable the mobile detection provider (if enabled)
                    HttpCapabilitiesBase.BrowserCapabilitiesProvider = new FiftyOne.Foundation.Mobile.Detection.MobileCapabilitiesProvider();
                }
                else
                {
                    //set BrowserCapabilitiesProvider to null because 51Degrees assembly always sets it to MobileCapabilitiesProvider
                    //it'll allow us to use default browserCaps.config file
                    HttpCapabilitiesBase.BrowserCapabilitiesProvider = null;
                }
            }
        }
        
        protected void Application_End()
        {
            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime",
                                                                                            BindingFlags.NonPublic
                                                                                            | BindingFlags.Static
                                                                                            | BindingFlags.GetField,
                                                                                            null,
                                                                                            null,
                                                                                            null);
            if (runtime == null)
                return;
            string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                                                                             BindingFlags.NonPublic
                                                                             | BindingFlags.Instance
                                                                             | BindingFlags.GetField,
                                                                             null,
                                                                             runtime,
                                                                             null);

            string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                                                                           BindingFlags.NonPublic
                                                                           | BindingFlags.Instance
                                                                           | BindingFlags.GetField,
                                                                           null,
                                                                           runtime,
                                                                           null);

            var logger = EngineContext.Current.Resolve<ILogger>();
            logger.Information(string.Format("Application ended\n message : {0}\n stack : {1}", shutDownMessage, shutDownStack));
        }
        
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            EnsureDatabaseIsInstalled();

            //TODO:AF- Comment out at production
           /* if (DataSettingsHelper.DatabaseIsInstalled() &&
                EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                MiniProfiler.Start();
            }*/
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            //TODO:AF- Comment out at production
           /* if (DataSettingsHelper.DatabaseIsInstalled() &&
                EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                //stop as early as you can, even earlier with MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
                MiniProfiler.Stop();
            }*/
        }
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        { 
            //we don't do it in Application_BeginRequest because a user is not authenticated yet
            SetWorkingCulture();
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            //log error
            LogException(exception);

            //process 404 HTTP errors
            var httpException = exception as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404)
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                if (!webHelper.IsStaticResource(this.Request))
                {
                    Response.Clear();
                    Server.ClearError();
                    Response.TrySkipIisCustomErrors = true;

                    // Call target Controller and pass the routeData.
                    IController errorController = EngineContext.Current.Resolve<Nop.Web.Controllers.CommonController>();

                    var routeData = new RouteData();
                    routeData.Values.Add("controller", "Common");
                    routeData.Values.Add("action", "PageNotFound");

                    errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }
            }
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            var _customerService = EngineContext.Current.Resolve<ICustomerService>();
            var _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _customerService.SaveCustomerAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, "");

        }

        protected void EnsureDatabaseIsInstalled()
        {
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            string installUrl = string.Format("{0}install", webHelper.GetStoreLocation());
            if (!webHelper.IsStaticResource(this.Request) &&
                !DataSettingsHelper.DatabaseIsInstalled() &&
                !webHelper.GetThisPageUrl(false).StartsWith(installUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                this.Response.Redirect(installUrl);
            }
        }

        protected void SetWorkingCulture()
        {
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            if (webHelper.IsStaticResource(this.Request))
                return;

            //keep alive page requested (we ignore it to prevnt creating a guest customer records)
            string keepAliveUrl = string.Format("{0}keepalive", webHelper.GetStoreLocation());
            if (webHelper.GetThisPageUrl(false).StartsWith(keepAliveUrl, StringComparison.InvariantCultureIgnoreCase))
                return;


            if (webHelper.GetThisPageUrl(false).StartsWith(string.Format("{0}afcockpit", webHelper.GetStoreLocation()),
                StringComparison.InvariantCultureIgnoreCase))
            {
                //admin area


                //always set culture to 'en-US'
                //we set culture of admin area to 'en-US' because current implementation of Telerik grid 
                //doesn't work well in other cultures
                //e.g., editing decimal value in russian culture
                var culture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            else
            {
                //public store
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                if (workContext.CurrentCustomer != null && workContext.WorkingLanguage != null)
                {
                    var culture = new CultureInfo(workContext.WorkingLanguage.LanguageCulture);
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
            }
        }
        protected void LogException(Exception exc)
        {
            if (exc == null)
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            try
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                logger.Error(exc.Message, exc, workContext.CurrentCustomer);
            }
            catch (Exception)
            {
                //don't throw new exception if occurs
            }
        }
        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom == "lgg")
            {
                return EngineContext.Current.Resolve<IWorkContext>().WorkingLanguage.LanguageCulture +
                    EngineContext.Current.Resolve<IWebHelper>().IsCurrentConnectionSecured().ToString();
            }
            return String.Empty;
        }
    
    }
}  