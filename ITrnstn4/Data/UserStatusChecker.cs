using ITrnstn4.Data;
using System.Security.Claims;

namespace ITrnstn4Old.Data
{
    public class UserStatusChecker
    {
        private readonly RequestDelegate _next;

        public UserStatusChecker(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId != null)
                {
                    var user = dbContext.Users.SingleOrDefault(u => u.Id.ToString() == userId);

                    if (user != null)
                    {
                        if (user.Status == "Blocked")
                        {
                            context.Response.Redirect("/Account/Login?blocked=true");
                            return;
                        }
                        else if (user == null)
                        {
                            context.Response.Redirect("/Account/Register?deleted=true");

                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

}
