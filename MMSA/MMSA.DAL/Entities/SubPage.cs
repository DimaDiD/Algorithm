using System;
using System.Collections.Generic;

namespace MMSA.DAL.Entities;

public partial class SubPage
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Page Page { get; set; } = null!;
}
