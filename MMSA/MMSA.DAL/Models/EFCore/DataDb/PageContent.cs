using System;
using System.Collections.Generic;

namespace MMSA.DAL.Models.EFCore.DataDb;

public partial class PageContent
{
    public int Id { get; set; }

    public string? Text { get; set; }

    public int PageId { get; set; }

    public string? TextType { get; set; }

    public string? CodeType { get; set; }

    public virtual Page Page { get; set; } = null!;
}
