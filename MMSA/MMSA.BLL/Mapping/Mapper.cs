using AutoMapper;
using MMSA.BLL.Dtos;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Mapping
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            CreateMap<Page, PageDTO>().ReverseMap();
            CreateMap<PageContent, PageContentDTO>().ReverseMap();
        }
    }
}
