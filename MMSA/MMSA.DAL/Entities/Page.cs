using System.ComponentModel.DataAnnotations;

namespace MMSA.DAL.Entities
{
    public class Page
    {
        [Key]
        public int Id { get; set; } = 0;

        public int PageId { get; set; }

        public string PageName { get; set; }

        public int TextId { get; set; }
    }
}
