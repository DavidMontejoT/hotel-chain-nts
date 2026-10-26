using HotelChain.Application.Hotels;
using HotelChain.Application.Images;
using Microsoft.Extensions.DependencyInjection;

namespace HotelChain.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IImageService, ImageService>();
        return services;
    }
}
