using System.ComponentModel.DataAnnotations;

namespace LuongVinhKhang.SachOnline.Models
{
    public class User
    {
        [Key]
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string Role { get; set; }
    }
}
