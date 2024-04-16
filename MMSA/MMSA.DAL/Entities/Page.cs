using System;
using System.Collections.Generic;

namespace MMSA.DAL.Entities;

public partial class Page
{
    public int Id { get; set; }

    public string PageName { get; set; } = null!;

    public virtual ICollection<SubPage> SubPages { get; set; } = new List<SubPage>();
}
