using System;
using System.Collections.Generic;

namespace MMSA.DAL.Models.EFCore.DataDb;

public partial class Page
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public string PageName { get; set; } = null!;

    public int? TextId { get; set; }
}
