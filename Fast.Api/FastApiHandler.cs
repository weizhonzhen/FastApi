﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FastData.Core.Repository;

namespace Fast.Api
{
    public class FastApiHandler
    {
        private readonly RequestDelegate next;
        private readonly OptionModel option;

        public FastApiHandler(RequestDelegate request, OptionModel _option=null) 
        { 
            next = request;
            option = _option;
        }

        public Task InvokeAsync(HttpContext context, IFastApi response, IFastRepository IFast)
        {
            return response.ContentAsync(context, IFast, next,option);
        }
    }
}
