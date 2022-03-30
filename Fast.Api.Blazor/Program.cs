global using FastAop.Core;
global using FastUntility.Core.Base;
using Fast.Api.Blazor.Filter;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); 
builder.Services.AddFastAop("Fast.Api.Blazor.Service", typeof(FastServiceFilter));

builder.Services.AddFastApi(a =>
{
    a.IsResource = false;
    a.dbFile = "db.json";
    a.mapFile = "map.json";
    a.dbKey = "Api";
});

var app = builder.Build();

app.UseExceptionHandler(error =>
{
    error.Run(async context =>
    {
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            BaseLog.SaveLog(contextFeature.Error.Message, "error");
            context.Response.ContentType = "application/json;charset=utf-8";
            context.Response.StatusCode = 200;
            var result = new Dictionary<string, object>();
            result.Add("success", false);
            result.Add("msg", contextFeature.Error.Message);
            await context.Response.WriteAsync(BaseJson.ModelToJson(result));
        }
    });
});

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
