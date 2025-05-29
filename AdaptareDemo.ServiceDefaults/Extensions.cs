using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

// 新增常用的 .NET Aspire 服務：服務發現、韌性、健康檢查，以及 OpenTelemetry。
// 此專案應被解決方案中的每個服務專案參考。
// 如需有關使用此專案的詳細資訊，請參閱 https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
	public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		builder.ConfigureOpenTelemetry();

		builder.AddDefaultHealthChecks();

		builder.Services.AddServiceDiscovery();

		builder.Services.ConfigureHttpClientDefaults(http =>
		{
			// 預設開啟韌性
			http.AddStandardResilienceHandler();

			// 預設開啟服務發現
			http.AddServiceDiscovery();
		});

		// 取消註解以下內容以限制服務發現允許的通訊協定。
		// builder.Services.Configure<ServiceDiscoveryOptions>(options =>
		// {
		//     options.AllowedSchemes = ["https"];
		// });

		return builder;
	}

	public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		builder.Logging.AddOpenTelemetry(logging =>
		{
			logging.IncludeFormattedMessage = true;
			logging.IncludeScopes = true;
		});

		builder.Services.AddOpenTelemetry()
			.WithMetrics(metrics => metrics
				.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddRuntimeInstrumentation())
			.WithTracing(tracing => tracing
				.AddSource("Adaptare.*")
				.AddSource(builder.Environment.ApplicationName)
				.AddAspNetCoreInstrumentation()
				// 取消註解以下行以啟用 gRPC 監控（需安裝 OpenTelemetry.Instrumentation.GrpcNetClient 套件）
				//.AddGrpcClientInstrumentation()
				.AddHttpClientInstrumentation());

		builder.AddOpenTelemetryExporters();

		return builder;
	}

	public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		builder.Services.AddHealthChecks()
			// 新增預設的活躍性檢查以確保應用程式有回應
			.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

		return builder;
	}

	public static WebApplication MapDefaultEndpoints(this WebApplication app)
	{
		// 在非開發環境中新增健康檢查端點有安全性上的考量。
		// 詳情請參閱 https://aka.ms/dotnet/aspire/healthchecks，於非開發環境啟用這些端點前請先了解相關細節。
		if (app.Environment.IsDevelopment())
		{
			// 所有健康檢查都必須通過，應用程式才會被視為已準備好接受流量
			app.MapHealthChecks("/health");

			// 只有標記為 "live" 的健康檢查必須通過，應用程式才會被視為存活
			app.MapHealthChecks("/alive", new HealthCheckOptions
			{
				Predicate = r => r.Tags.Contains("live")
			});
		}

		return app;
	}

	private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

		if (useOtlpExporter)
		{
			builder.Services.AddOpenTelemetry().UseOtlpExporter();
		}

		// 取消註解以下內容以啟用 Azure Monitor 匯出器（需安裝 Azure.Monitor.OpenTelemetry.AspNetCore 套件）
		//if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
		//{
		//    builder.Services.AddOpenTelemetry()
		//       .UseAzureMonitor();
		//}

		return builder;
	}
}