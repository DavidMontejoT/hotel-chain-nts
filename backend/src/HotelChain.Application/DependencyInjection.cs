using HotelChain.Application.Hotels;
using Microsoft.Extensions.DependencyInjection;

namespace HotelChain.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IHotelService, HotelService>();
        return services;
    }
}
