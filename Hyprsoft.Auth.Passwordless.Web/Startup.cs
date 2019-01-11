﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hyprsoft.Auth.Passwordless.Web
{
    public class Startup
    {
        #region Constructors

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        #endregion

        #region Methods

        public void ConfigureServices(IServiceCollection services)
        {
            AuthenticationServiceOptions authenticationServiceOptions = new AuthenticationServiceOptions();
            services.AddDbContext<PasswordlessAuthDbContext>(options =>
            {
                if (HostingEnvironment.EnvironmentName.Equals("Test", StringComparison.CurrentCultureIgnoreCase))
                {
                    authenticationServiceOptions.BearerAccessTokenLifespan = TimeSpan.FromSeconds(3);
                    authenticationServiceOptions.BearerRefreshTokenLifespan = TimeSpan.FromSeconds(5);
                    options.UseInMemoryDatabase("Hyprsoft.Auth.Passwordless");
                }
                else
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), o => o.EnableRetryOnFailure());
            });

            services.AddIdentity<PasswordlessAuthIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<PasswordlessAuthDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = authenticationServiceOptions.OtpTokenLifespan);
            services.ConfigureApplicationCookie(options => options.ForwardForbid = JwtBearerDefaults.AuthenticationScheme);
            services.AddHttpsRedirection(options => options.HttpsPort = 443);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = authenticationServiceOptions.BearerTokenClockSkew,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authenticationServiceOptions.BearerTokenIssuer,
                    ValidAudience = authenticationServiceOptions.BearerTokenAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationServiceOptions.BearerTokenSecurityKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            context.Response.Headers.Add("Token-Expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddSingleton(authenticationServiceOptions);
            services.AddScoped<AuthenticationService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }

        #endregion
    }
}