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
            CreateMap<SubPage, SubPageDTO>().ReverseMap();
            CreateMap<PageContent, PageContentDTO>()
                .ForMember(dto => dto.Topics, options => options.MapFrom(entity =>
                    entity.Topics.Select(topic => topic.Name).OrderBy(name => name).ToList()))
                .ReverseMap()
                .ForMember(entity => entity.Topics, options => options.Ignore());
        }
    }
}
