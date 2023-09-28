using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace DeskManagementApp.Middlewares
{
    public class SystemConfiguredMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        public SystemConfiguredMiddleware(IMemoryCache cache, RequestDelegate next) 
        {
            _cache = cache;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool systemConfigured = false ;

            if (_cache.Get<bool>("SystemConfigured") != null)
            {
                systemConfigured = _cache.Get<bool>("SystemConfigured");
            }

            if (systemConfigured)
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 403; 
                await context.Response.WriteAsync("System not yet configured.");
            }
        }
    }
}
