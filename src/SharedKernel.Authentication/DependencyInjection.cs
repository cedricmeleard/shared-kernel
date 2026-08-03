using Authentication.Abstractions;
using Authentication.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        var authOptions = new AuthenticationOptions();
        configuration.GetSection(AuthenticationOptions.SectionName).Bind(authOptions);

        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;
                options.Audience = authOptions.Audience;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;

                if (!string.IsNullOrWhiteSpace(authOptions.MetadataAddress))
                    options.MetadataAddress = authOptions.MetadataAddress;
            });

        services.AddAuthorization();

        return services;
    }
}