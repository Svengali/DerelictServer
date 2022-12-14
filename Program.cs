

global using Imm = System.Collections.Immutable.ImmutableInterlocked;


using auth;

using lib;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

using svc;

using System.Text.Json.Serialization;

using WebApi.Helpers;

//using System.Windows.Forms;


//Task.Run( () => Application.Run( new Form1() ) );


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<svc.IUser, svc.User>();
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

builder.Services.AddAuthorization( options => {
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


var configSection = app.Configuration.GetSection("UserSettings");

var userSettings = app.Configuration.Get<svc.UserSettings>();

svc.Data<svc.UserData>.BaseDir = userSettings.UserDir;
svc.Data<svc.TokenToUser>.BaseDir = userSettings.TokenDir;

svc.Data<svc.UserData>.BackupBaseDir = userSettings.UserBackupDir;
svc.Data<svc.TokenToUser>.BackupBaseDir = userSettings.TokenBackupDir;

svc.Data<svc.UserData>.FnKey = ( d ) => d.Email;
svc.Data<svc.TokenToUser>.FnKey = ( d ) => d.UserId;

Util.checkAndAddDirectory( userSettings.UserDir );
Util.checkAndAddDirectory( userSettings.TokenDir );

Util.checkAndAddDirectory( userSettings.UserBackupDir );
Util.checkAndAddDirectory( userSettings.TokenBackupDir );

//Keeping this as a sleep loop 
while( !appTask.IsCompleted )
{
	Thread.Sleep(1000);
}

