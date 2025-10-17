using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MoneyTracker.Filters;

public class RequireJwtCookieAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var hasJwt = context.HttpContext.Request.Cookies.ContainsKey("jwt");
        if (!hasJwt)
        {
            context.Result = new RedirectToPageResult("/Account/Login");
        }
    }
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}


