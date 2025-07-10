namespace LuongVinhKhang.SachOnline.Models
{
    public class ChuDe
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Product> Product { get; set; }
    }
}