using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DeskManagementApp.Middlewares
{
    public class SystemNotConfiguredMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache; 
        public SystemNotConfiguredMiddleware(IMemoryCache cache, RequestDelegate next)
        {
            _cache = cache;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            bool systemConfigured = _cache.Get<bool>("SystemConfigured");

            if (!systemConfigured)
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("System already configured.");
            }
        }
    }
}
