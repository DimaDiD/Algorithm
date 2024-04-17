using System;
using System.Collections.Generic;

namespace MMSA.DAL.Entities;

public partial class PageContent
{
    public int Id { get; set; }

    public string? Text { get; set; }

    public int SubPageId { get; set; }

    public string? TextType { get; set; }

    public string? CodeType { get; set; }

    public int? ContentLocation { get; set; }

    public int? PageId { get; set; }
}
