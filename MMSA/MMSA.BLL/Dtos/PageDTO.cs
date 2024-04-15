namespace MMSA.BLL.Dtos
{
    public class PageDTO
    {
        public int Id { get; set; }

        public int PageId { get; set; }

        public string PageName { get; set; } = null!;

        public int? TextId { get; set; }

        public List<PageDTO> PageContens { get; set; }  = new List<PageDTO>();
    }
}
