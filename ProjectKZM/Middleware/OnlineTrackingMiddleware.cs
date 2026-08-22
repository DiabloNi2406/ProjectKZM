using ProjectKZM.Services;
using System.Security.Claims;

namespace ProjectKZM.Middleware
{
    public class OnlineTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public OnlineTrackingMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(
            HttpContext context,
            OnlineUserService onlineUserService)
        {
            // =================================================
            // KHÔNG ĐẾM FILE TĨNH
            // =================================================

            string path =
                context.Request.Path.Value
                ?? string.Empty;


            if (!IsStaticFile(path))
            {
                // =============================================
                // USER ĐĂNG NHẬP
                // =============================================

                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var claim =
                        context.User.FindFirst(
                            ClaimTypes.Email);

                    if (claim != null)
                    {
                        var userId =
                            context.User.FindFirst(
                                ClaimTypes.NameIdentifier);


                        if (userId != null &&
                            int.TryParse(
                                userId.Value,
                                out int maNguoiDung))
                        {
                            onlineUserService.TrackUser(
                                maNguoiDung);
                        }
                        else
                        {
                            // Nếu cookie hiện tại chưa có
                            // MaNguoiDung thì dùng email làm ID
                            onlineUserService.TrackUserByKey(
                                claim.Value);
                        }
                    }
                }

                // =============================================
                // KHÁCH VÃNG LAI
                // =============================================

                else
                {
                    string sessionId =
                        context.Session.Id;

                    onlineUserService.TrackGuest(
                        sessionId);
                }
            }


            await _next(context);
        }


        private static bool IsStaticFile(
            string path)
        {
            string[] extensions =
            {
                ".css",
                ".js",
                ".png",
                ".jpg",
                ".jpeg",
                ".gif",
                ".svg",
                ".ico",
                ".webp",
                ".woff",
                ".woff2",
                ".ttf"
            };


            return extensions.Any(
                x => path.EndsWith(
                    x,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}