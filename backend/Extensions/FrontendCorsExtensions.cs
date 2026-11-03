namespace LilyWhiteMap.Api.Extensions;

public static class FrontendCorsExtensions
{
    public const string FrontendPolicyName = "Frontend";

    /// <summary>Allows the statically-hosted Next.js frontend (local + Amplify + production domain) to call this API with credentials (for the visitor-tracking cookie).</summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicyName, policy =>
                policy.WithOrigins(
                        "http://localhost:3000",
                        "http://127.0.0.1:3000",
                        "https://master.d3uj4k8iqldq7q.amplifyapp.com",
                        "https://tottenhamplayersmap.com",
                        "https://www.tottenhamplayersmap.com"
                        )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }
}
