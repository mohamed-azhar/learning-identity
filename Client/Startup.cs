using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "mifco.web";
                options.DefaultSignInScheme = "mifco.web";
                options.DefaultChallengeScheme = "challenge.web";
            }).AddCookie("mifco.web")
            .AddOAuth("challenge.web", options =>
            {
                options.SaveTokens = true;
                options.CallbackPath = "/oauth/callback";
                options.ClientId = "c88bbe72-063a-476f-870c-03513e35be5b";
                options.ClientSecret = "not-your-average-secret-value-for-your-web-application";
                options.AuthorizationEndpoint = "https://localhost:44307/oauth/authorize";
                options.TokenEndpoint = "https://localhost:44307/oauth/token";

                options.Events = new OAuthEvents()
                {
                    OnCreatingTicket = context =>
                    {
                        var token = context.AccessToken;
                        var base64Payload = token.Split('.')[1];
                        var bytes = Convert.FromBase64String(base64Payload);
                        var jsonPayload = Encoding.UTF8.GetString(bytes);

                        var claims = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPayload);

                        //this throws an error (due to nbf & exp being integers)
                        //var claims = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonPayload);

                        foreach (var claim in claims)
                        {
                            context.Identity.AddClaim(new Claim(claim.Key, claim.Value));
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHttpClient();

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
