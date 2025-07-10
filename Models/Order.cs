using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongVinhKhang.SachOnline.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey("User")]
        public int MaKH { get; set; }
        public User User { get; set; }

        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public DateTime NgayDat { get; set; }
        public DateTime? NgayGiao { get; set; }
        public string? TrangThai { get; set; }
        public decimal TongTien { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
