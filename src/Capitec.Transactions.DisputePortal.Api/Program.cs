using Capitec.Transactions.DisputePortal.Api.Dependencies;
using Capitec.Transactions.DisputePortal.Api.Middleware;
using Capitec.Transactions.DisputePortal.Api.Swagger;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ApiConfiguration apiConfiguration = ApiConfigurationValidator.Validate(builder.Configuration);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

ControllerRegistration.Register(builder.Services);
ApiVersioningRegistration.Register(builder.Services);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(SwaggerRegistration.Register);
AuthenticationRegistration.Register(builder.Services, apiConfiguration.Authentication);
DependencyRegistration.Register(builder.Services, builder.Configuration, apiConfiguration);
HealthCheckRegistration.Register(builder.Services);

WebApplication app = builder.Build();
bool isLocalOrDevelopment = ApiEnvironments.IsLocalOrDevelopment(app.Environment);

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (isLocalOrDevelopment)
{
    SwaggerUiRegistration.UseVersionedSwaggerUi(app);
    app.MapGet("/", () => Results.Redirect(SwaggerUiRegistration.Route)).ExcludeFromDescription().AllowAnonymous();
}

app.UseAuthentication();
app.UseAuthorization();

HealthCheckRegistration.Map(app);
app.MapControllers();

await app.RunAsync();

public partial class Program;
