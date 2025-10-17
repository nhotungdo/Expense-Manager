using System;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    public void OnGet()
    {
        Response.Cookies.Delete("jwt");
    }
}


