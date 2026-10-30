using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos
{
    public class PageContentDTO
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public int PageId { get; set; }

        public int? SubPageId { get; set; }

        [Required, MaxLength(200)]
        public string TextType { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string CodeType { get; set; } = string.Empty;

        public int ContentLocation { get; set; }

        [MaxLength(15)]
        public List<string> Topics { get; set; } = new();
    }
}
