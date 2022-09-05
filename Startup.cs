using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace game
{
  public class DerelictSettings
  {

  }


  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddRazorPages();

			/*
			services
					.AddAuthentication()
					.AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", options => { });
			*/

			services.AddAuthorization(options =>
			{
				options.AddPolicy("BasicAuth", new AuthorizationPolicyBuilder("BasicAuth").RequireAuthenticatedUser().Build());
			});


		}

		public void Configure(WebApplication app, IWebHostEnvironment env)
    {
      /*
      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      */

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }




      //app.UseHttpsRedirection();
      //app.UseStaticFiles();

      app.UseRouting();
      app.UseMvcWithDefaultRoute();
      app.MapRazorPages();
      app.Run();
    }
  }
}
