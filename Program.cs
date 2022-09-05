


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

using svc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHostedService<svc.Edge>();
builder.Services.AddHostedService<svc.Player>();


builder.Services
		.AddAuthentication()
		.AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", options => { });


builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("BasicAuth", new AuthorizationPolicyBuilder("BasicAuth").RequireAuthenticatedUser().Build());
});


builder.Services.AddMvc(cfg => {
  cfg.EnableEndpointRouting = false;

  });


/*
IConfiguration cfg = builder.Configuration;
IWebHostEnvironment env = builder.Environment;

env.
*/

//builder.Configuration.
builder.Services.AddLogging(builder =>
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "hh:mm:ss ";
        options.UseUtcTimestamp = true;
    })
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseMvcWithDefaultRoute();

app.MapControllers();

app.MapDefaultControllerRoute();


app.Use(async (context, next) =>
{
  // Do work that can write to the Response.
  await next.Invoke();
  // Do logging or other work that doesn't write to the Response.
});


app.Run();


