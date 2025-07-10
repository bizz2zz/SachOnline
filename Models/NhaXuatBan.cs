namespace LuongVinhKhang.SachOnline.Models
{
    public class NhaXuatBan
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Product> Product { get; set; }
    }
}
