using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using CraigslistSearch.Function.Services;

[assembly: FunctionsStartup(typeof(CraigslistSearch.Function.Startup))]

namespace CraigslistSearch.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<SearchService, SearchService>();
        }
    }
}