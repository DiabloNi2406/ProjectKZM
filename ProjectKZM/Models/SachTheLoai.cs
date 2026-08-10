namespace ProjectKZM.Models
{
    public class SachTheLoai
    {
        public int MaSach { get; set; }

        public int MaTheLoai { get; set; }

        public Sach Sach { get; set; } = null!;

        public TheLoai TheLoai { get; set; } = null!;
    }
}