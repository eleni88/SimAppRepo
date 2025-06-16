namespace SimulationProject.Helper
{
    public class CsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public CsrfValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check in methods POST, PUT, DELETE
            if (HttpMethods.IsPost(context.Request.Method) ||
                HttpMethods.IsPut(context.Request.Method) ||
                HttpMethods.IsDelete(context.Request.Method))
            {
                var csrfFromHeader = context.Request.Headers["X-CSRF-TOKEN"].FirstOrDefault();
                var csrfFromCookie = context.Request.Cookies["CsrfCookie"];

                if (string.IsNullOrWhiteSpace(csrfFromHeader) ||
                    csrfFromHeader != csrfFromCookie)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid or missing CSRF token.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
