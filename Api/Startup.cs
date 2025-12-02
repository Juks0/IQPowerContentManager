using System;
using System.Web.Http;
using System.Web.Http.Cors;
using Owin;
using System.Reflection;
using System.IO;
using Swashbuckle.Application;

namespace IQPowerContentManager.Api
{
    /// <summary>
    /// Klasa konfiguracyjna OWIN dla Web API
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Konfiguruje pipeline OWIN i Web API
        /// </summary>
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            // Enable CORS - pozwól na wszystkie źródła (dla frontendu)
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Default route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // JSON formatter
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Swagger configuration - MUSI być przed UseWebApi
            // Dla OWIN z Web API 2, Swagger rejestruje routing automatycznie
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "IQPower Content Manager API")
                        .Description("REST API do zarządzania konfiguracją Assetto Corsa")
                        .Contact(cc => cc
                            .Name("IQPower Content Manager")
                            .Email("support@iqpower.com"));

                    // Include XML comments if available
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }

                    // Group endpoints by controller
                    c.GroupActionsBy(apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName);
                })
                .EnableSwaggerUi(c =>
                {
                    c.DocumentTitle("IQPower Content Manager API");
                    c.DocExpansion(DocExpansion.List);
                    c.DisableValidator();
                });

            // WAŻNE: Zainicjalizuj konfigurację przed użyciem
            // To zapewnia, że wszystkie routingi są poprawnie zarejestrowane
            config.EnsureInitialized();

            // Loguj zarejestrowane kontrolery (dla debugowania)
            var controllerTypes = config.Services.GetHttpControllerTypeResolver()
                .GetControllerTypes(config.Services.GetAssembliesResolver());
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [API] Zarejestrowane kontrolery:");
            foreach (var controllerType in controllerTypes)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [API]   - {controllerType.Name} ({controllerType.Namespace})");
            }

            // Załaduj zapisany stan aplikacji przy starcie
            StateHelper.LoadSavedState();

            // Use Web API - routing Swagger jest już zarejestrowany w config przez EnableSwagger/EnableSwaggerUi
            app.UseWebApi(config);
        }
    }
}

