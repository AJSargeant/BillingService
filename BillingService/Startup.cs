using System;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BillingModel;
using Microsoft.EntityFrameworkCore;

namespace BillingService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDbContext<BillingContext>(options => options.UseSqlServer(Configuration.GetConnectionString("BillingDatabase")));

            //TODO: Store secret key etc appropriately
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MY TOP SECRET TEST KEY")),
                ValidateIssuer = true,
                ValidIssuer = "issuer",
                ValidateAudience = true,
                ValidAudience = "audience",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
                o.RequireHttpsMetadata = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                app.Use(async (context, next) =>
                {
                    StringValues passedToken = new StringValues();
                    context.Request.Headers.TryGetValue("Authorization", out passedToken);
                    if (passedToken.ToString() == "")
                    {
                        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MY TOP SECRET TEST KEY"));
                        var claims = new Claim[] {
                            new Claim(ClaimTypes.NameIdentifier, "12"),
                            new Claim(ClaimTypes.Name, "Joe"),
                            new Claim(ClaimTypes.Role, "Customer")
                        };

                        var token = new JwtSecurityToken(
                            issuer: "issuer",
                            audience: "audience",
                            claims: claims,
                            notBefore: DateTime.Now.Subtract(new TimeSpan(2, 1, 1)),
                            expires: DateTime.Now.AddDays(7),
                            signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
                        );
                        context.Request.Headers.Add("Authorization", "Bearer " + new JwtSecurityTokenHandler().WriteToken(token));
                    }

                    await next.Invoke();
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
