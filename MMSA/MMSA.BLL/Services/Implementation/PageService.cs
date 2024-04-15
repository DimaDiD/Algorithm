﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation
{
    public class PageService: IPageService
    {
        private readonly IPageRepository _pageRepository;
        private readonly IMapper _mapper;
        public PageService(IPageRepository pageRepository,
            IMapper mapper) 
        {
            _pageRepository = pageRepository;
            _mapper = mapper;
        }

        public async Task<PageDTO> CreateAsync(PageDTO page)
        {
            try
            {
                var newPage = _mapper.Map<Page>(page);

                await _pageRepository.InsertAsync(newPage, true);

                return page;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in CreateAsync page method. Message: {exception.Message}");
            }
        }

        public async Task<List<PageDTO>> GetAllAsync()
        {
            try
            {
                var dbPages = await _pageRepository.GetAllAsync(include: x => x.Include(c => c.PageContents));
                var pages = _mapper.Map<List<PageDTO>>(dbPages);
                return pages;
            }
            catch (Exception exception)
            {
                throw new Exception($"Error in create GetAllAsync page method. Message: {exception.Message}");
            }
        }
    }
}
