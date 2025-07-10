using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuongVinhKhang.SachOnline.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }

        [Column("NhaXuatBan_Id")]
        public int? NhaXuatBanId { get; set; }

        [ValidateNever]
        public NhaXuatBan NhaXuatBan { get; set; }

        [Column("ChuDe_Id")]
        public int? ChuDeId { get; set; }

        [ValidateNever]
        public ChuDe ChuDe { get; set; }
        [ValidateNever]
        public string Image { get; set; }

        [Column("soluongban")]
        public int SoLuongBan { get; set; }

        public string MoTa { get; set; }
    }
}