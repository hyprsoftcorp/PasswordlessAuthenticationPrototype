using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Pwdless Authentication API",
                    Version = "v1",
                    Description = "Password-less Authentication REST API.",
                    Contact = new Contact { Name = "Hal Blakeslee", Email = "hal@hyprsoft.com", Url = "http://www.hyprsoft.com/" }
                });
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Value below should be in the form: \"Bearer &lt;your token&gt;\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", new string[] { } } });
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Hyprsoft.Auth.Passwordless.App).Assembly.GetName().Name}.xml"));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            });
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
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pwdless Authentication API V1"));
            app.UseMvcWithDefaultRoute();
        }

        #endregion
    }
}
