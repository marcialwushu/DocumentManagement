using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using Microsoft.Extensions.Logging;

namespace DocumentManagement.Infrastructure.Observability
{
    public static class OpenTelemetryConfiguration
    {
        public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services, string serviceName)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector();

            // Configure Tracing
            services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri("http://localhost:4317");
                    }));

            // Configure Metrics
            services.AddOpenTelemetry()
                .WithMetrics(builder => builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri("http://localhost:4317");
                    }));

            return services;
        }

        public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder builder, string serviceName)
        {
            builder.AddOpenTelemetry(options =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault()
                    .AddService(serviceName)
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector();

                options.SetResourceBuilder(resourceBuilder);
                options.AddOtlpExporter(opts =>
                {
                    opts.Endpoint = new Uri("http://localhost:4317");
                });
            });

            return builder;
        }
    }
}