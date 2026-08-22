using System.Collections.Concurrent;

namespace ProjectKZM.Services
{
    public class OnlineUserService
    {
        private readonly ConcurrentDictionary<string, DateTime>
            _onlineUsers = new();

        private int _guestVisitsToday = 0;
        private int _totalVisitsToday = 0;

        // =====================================================
        // THỜI GIAN ĐƯỢC XEM LÀ ĐANG ONLINE
        // =====================================================

        private static readonly TimeSpan OnlineTimeout =
            TimeSpan.FromMinutes(5);


        // =====================================================
        // GHI NHẬN REQUEST
        // =====================================================

        public void TrackGuest(string sessionId)
        {
            Cleanup();

            _onlineUsers[$"guest:{sessionId}"] =
                DateTime.UtcNow;

            Interlocked.Increment(
                ref _guestVisitsToday);

            Interlocked.Increment(
                ref _totalVisitsToday);
        }


        public void TrackUser(int maNguoiDung)
        {
            Cleanup();

            _onlineUsers[$"user:{maNguoiDung}"] =
                DateTime.UtcNow;

            Interlocked.Increment(
                ref _totalVisitsToday);
        }


        // =====================================================
        // SỐ KHÁCH VÃNG LAI ĐANG ONLINE
        // =====================================================

        public int GetOnlineGuestCount()
        {
            Cleanup();

            return _onlineUsers
                .Count(x =>
                    x.Key.StartsWith("guest:"));
        }


        // =====================================================
        // SỐ TÀI KHOẢN ĐANG ONLINE
        // =====================================================

        public int GetOnlineUserCount()
        {
            Cleanup();

            return _onlineUsers
                .Count(x =>
                    x.Key.StartsWith("user:"));
        }


        // =====================================================
        // LƯỢT KHÁCH VÃNG LAI HÔM NAY
        // =====================================================

        public int GetGuestVisitsToday()
        {
            return _guestVisitsToday;
        }


        // =====================================================
        // TỔNG LƯỢT TRUY CẬP HÔM NAY
        // =====================================================

        public int GetTotalVisitsToday()
        {
            return _totalVisitsToday;
        }


        // =====================================================
        // LẤY THỜI GIAN ONLINE
        // =====================================================

        public Dictionary<string, DateTime>
            GetOnlineUsers()
        {
            Cleanup();

            return _onlineUsers
                .ToDictionary(
                    x => x.Key,
                    x => x.Value);
        }

        public void TrackUserByKey(string key)
        {
            Cleanup();

            _onlineUsers[$"user:{key}"] =
                DateTime.UtcNow;

            Interlocked.Increment(
                ref _totalVisitsToday);
        }


        // =====================================================
        // XÓA NGƯỜI KHÔNG HOẠT ĐỘNG
        // =====================================================

        private void Cleanup()
        {
            DateTime limit =
                DateTime.UtcNow - OnlineTimeout;


            foreach (var item in _onlineUsers)
            {
                if (item.Value < limit)
                {
                    _onlineUsers.TryRemove(
                        item.Key,
                        out _);
                }
            }
        }
    }
}