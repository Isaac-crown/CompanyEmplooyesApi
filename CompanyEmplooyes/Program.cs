using AspNetCoreRateLimit;
using CompanyEmplooyes.Extension;
using CompanyEmplooyes.Utility;
using Contracts.Interface;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Presentation;
using Presentation.ActionFilters;
using Service.DataShaping;
using Shared.DataTransferObjects;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers(config => {
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile
    {
        Duration =120
    });
}).AddXmlDataContractSerializerFormatters()
 .AddCustomCSVFormatter()
.AddApplicationPart(typeof(AssemblyReference).Assembly);
NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson()
.Services.BuildServiceProvider()
.GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
.OfType<NewtonsoftJsonPatchInputFormatter>().First();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddScoped<ValidationFilterAttribute>();

builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();
builder.Services.AddCustomMediaTypes();

builder.Services.AddScoped<ValidateMediaTypeAttribute>();
builder.Services.ConfigureVersioning();

builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();

builder.Services.AddMemoryCache();
builder.Services.ConfigureRateLimitingOptions();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication();
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJWT(builder.Configuration);


builder.Services.AddControllers();


LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),
"/nlog.config"));

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);
if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});
app.UseIpRateLimiting();
app.UseCors("CorsPolicy");
app.UseResponseCaching();
app.UseHttpCacheHeaders();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
