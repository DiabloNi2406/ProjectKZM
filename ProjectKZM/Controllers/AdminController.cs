using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectKZM.Services;
using ProjectKZM.ViewModels;

namespace ProjectKZM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly OnlineUserService _onlineUserService;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public AdminController(
            OnlineUserService onlineUserService)
        {
            _onlineUserService = onlineUserService;
        }


        // =====================================================
        // TRANG QUẢN LÝ ADMIN
        // =====================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // =====================================================
        // THỐNG KÊ TRUY CẬP
        // =====================================================

        [HttpGet]
        public IActionResult TruyCap()
        {
            var model =
                new TruyCapViewModel
                {
                    KhachVangLaiOnline =
                        _onlineUserService
                            .GetOnlineGuestCount(),

                    TaiKhoanOnline =
                        _onlineUserService
                            .GetOnlineUserCount(),

                    LuotKhachVangLaiHomNay =
                        _onlineUserService
                            .GetGuestVisitsToday(),

                    TongLuotTruyCapHomNay =
                        _onlineUserService
                            .GetTotalVisitsToday()
                };


            return View(model);
        }
    }
}