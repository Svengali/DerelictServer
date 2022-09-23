

global using Imm = System.Collections.Immutable.ImmutableInterlocked;


using auth;


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

using svc;

using System.Text.Json.Serialization;

using WebApi.Helpers;

//using System.Windows.Forms;


//Task.Run( () => Application.Run( new Form1() ) );


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<svc.IPlayer, svc.Player>();
builder.Services.AddSingleton<svc.IEdge, svc.Edge>();

builder.Services.AddCors();


builder.Services.AddControllers().AddJsonOptions( x =>
{
	// serialize enums as strings in api responses (e.g. Role)
	x.JsonSerializerOptions.Converters.Add( new JsonStringEnumConverter() );
} );

//builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions();


// configure DI for application services
builder.Services.AddScoped<IJwtUtils, JwtUtils>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

/*
builder.Services
		.AddAuthentication()
		.AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", options => { });
*/

builder.Services.AddAuthorization( options =>
{
	options.AddPolicy( "BasicAuth", new AuthorizationPolicyBuilder( "BasicAuth" ).RequireAuthenticatedUser().Build() );
} );


builder.Services.AddMvc( cfg =>
{
	cfg.EnableEndpointRouting = false;

} );


/*
IConfiguration cfg = builder.Configuration;
IWebHostEnvironment env = builder.Environment;

env.
*/

//builder.Configuration.
builder.Services.AddLogging( builder =>
		builder.AddSimpleConsole( options =>
		{
			options.IncludeScopes = true;
			options.SingleLine = true;
			options.TimestampFormat = "hh:mm:ss ";
			options.UseUtcTimestamp = true;
		} )
);





var app = builder.Build();

// Configure the HTTP request pipeline.
if( app.Environment.IsDevelopment() )
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseMvcWithDefaultRoute();


app.MapDefaultControllerRoute();

app.MapControllers();

// global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();

// custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();



app.Use( async ( context, next ) =>
{
	// Do work that can write to the Response.
	await next.Invoke();
	// Do logging or other work that doesn't write to the Response.
} );

var appTask = app.RunAsync();


var configSection = app.Configuration.GetSection("PlayerSettings");

var playerSettings = app.Configuration.Get<svc.PlayerSettings>();

svc.Data<svc.PlayerData>.BaseDir = playerSettings.PlayerDir;
svc.Data<svc.TokenToPlayer>.BaseDir = playerSettings.TokenDir;

//Keeping this as a sleep loop 
while( !appTask.IsCompleted )
{
	Thread.Sleep(1000);
}

