﻿using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations
{
    public class PageRepository : BaseRepository<Page>, IPageRepository
    {
        public PageRepository(AlgorithmDataContext dataContext) : base(dataContext)
        {

        }
    }
}
