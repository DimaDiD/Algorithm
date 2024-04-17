namespace MMSA.BLL.Dtos
{
    public class PageDTO
    {
        public int Id { get; set; }

        public string PageName { get; set; }

        public List<SubPageDTO> SubPages { get; set; }
    }
}
