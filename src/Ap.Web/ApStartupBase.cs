﻿using Microsoft.AspNetCore.Builder;
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
                    jsonOptions.SerializerSettings.DateFormatString = Utils.ApShortDateFormat;
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
            options.Filters.Add(typeof(UnhandledExceptionAttribute));
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
        /// 配置其它服务
        /// </summary>
        /// <param name="services"></param>
        protected abstract void InternalConfigureServices(IServiceCollection services);

        /// <summary>
        /// 是否开启RequestLog
        /// </summary>
        protected virtual bool EnableRequestLog => true;

        /// <summary>
        /// 是否开启身份验证
        /// </summary>
        protected virtual bool EnableAuthentication => true;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
        {
            // loggerFactory.AddConsole(config.GetSection("Logging"));
            // loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties =true });
            // loggerFactory.ConfigureNLog("nlog.config");
            
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseExceptionless(config);

            if (EnableAuthentication)
            {
                app.UseAuthentication();
            }

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
