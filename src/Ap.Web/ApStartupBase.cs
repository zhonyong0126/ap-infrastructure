using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Ap.Web.Authentication;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Linq;
using Exceptionless;
using Ap.Infrastructure;

namespace Ap.Web
{
    public abstract class ApStartupBase
    {
        private readonly IConfiguration _config;

        public ApStartupBase(IHostingEnvironment env, IConfiguration configuration)
        {
            _config = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //注册Mvc
            services.AddMvc(ConfigureMvcOptions)
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.SerializerSettings.DateFormatString =Utils.ApShortDateFormat ;
                }); 

            //注册Log
            services.AddLogging();

            //注册Swagger
            if (EnableSwagger)
            {
                services.AddSwaggerGen(ConfigureSwaggerGen);
            }

            //注册Options
            services.AddOptions();
            services.Configure<ApiResultWrapperSettings>(_config.GetSection("ApiResultWrappper"));

            //注册Authorization
            services.AddAuthorization(ConfigureAuthorization);
            AddAuthorizationHandler(services);

            //注册
            ConfigureJwtBearerAuthentication(new JwtBeareAuthenticationOptions()
            {
                TokenExtractorFactoryType = typeof(BearerTokenExtractorFactory)
            }, services);

            InternalConfigureServices(services);

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// 配置MVC
        /// </summary>
        /// <param name="options">选项</param>
        protected virtual void ConfigureMvcOptions(MvcOptions options)
        {
            options.Filters.Add(typeof(CustomizedApiResultFilter));
            options.Filters.Add(typeof(BizExceptionFilterAttribute));
        }

        /// <summary>
        /// 是否启用Swagger
        /// </summary>
        protected virtual bool EnableSwagger => true;

        /// <summary>
        /// 配置SwaggerGen
        /// </summary>
        /// <param name="options">选项</param>
        protected virtual void ConfigureSwaggerGen(SwaggerGenOptions options)
        {
            //Add OperationFitlers
            //options.OperationFilter<AuthResponsesOperationFilter>();
        }

        /// <summary>
        /// 配置Authorization
        /// </summary>
        /// <param name="options">选项</param>
        /// <param name="services">services</param>
        protected virtual void ConfigureAuthorization(AuthorizationOptions options)
        {
            options.AddPolicy(WebCommonConsts.Authorization_Policy_Loggedin, policy => policy.AddRequirements(new LoginRequirement()));
        }

        /// <summary>
        /// 添加Authorization Handlers
        /// </summary>
        /// <param name="services"></param>
        protected virtual void AddAuthorizationHandler(IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, LoggedinAuthroizationHandler>();
        }

        private bool _jwtBearerAuthenticationEnabled = false;
        /// <summary>
        /// 配置JwtBeareAuthentication
        /// </summary>
        /// <param name="options"></param>
        protected virtual void ConfigureJwtBearerAuthentication(JwtBeareAuthenticationOptions options, IServiceCollection services)
        {
            if (null == options)
            {
                _jwtBearerAuthenticationEnabled = false;
                return;
            }
            _jwtBearerAuthenticationEnabled = true;
            services.AddSingleton<HeaderTokenExtractor>();
            services.AddSingleton<CookieTokenExtractor>();
            services.AddSingleton<QueryStringTokenExtractor>();
            services.AddSingleton(typeof(IAuthenticationTicketProviderFactory), options.TicketProviderFactoryType);
            services.AddSingleton(typeof(IBearerTokenExtractorFactory), options.TokenExtractorFactoryType);
        }


        /// <summary>
        /// 配置其它服务
        /// </summary>
        /// <param name="services"></param>
        protected abstract void InternalConfigureServices(IServiceCollection services);

        /// <summary>
        /// 是否开启RequestLog
        /// </summary>
        protected virtual bool EnableRequestLog => true;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(config.GetSection("Logging"));
            //loggerFactory.AddDebug();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (_jwtBearerAuthenticationEnabled)
            {
                app.UseMiddleware<BearerAuthenticationMiddleware>(new object[]{ Options.Create(new BearerAuthenticationOptions()
                {
                    AuthenticationScheme="Bearer",
                    AutomaticChallenge=true,
                    AutomaticAuthenticate=true
                })});
            }

            app.UseExceptionless(config);

            app.UseMvc();

            if (EnableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(ConfigureSwaggerUI);
            }
        }

        protected virtual void ConfigureSwaggerUI(SwaggerUIOptions options)
        {
            options.RoutePrefix = "doc";
        }
    }
}
