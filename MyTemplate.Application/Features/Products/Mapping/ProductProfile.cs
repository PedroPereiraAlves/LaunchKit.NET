using AutoMapper;
using MyTemplate.Application.DTOs;
using MyTemplate.Application.Features.Products.Commands;
using MyTemplate.Domain.Entities;

namespace MyTemplate.Application.Features.Products.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap().MaxDepth(64);
        CreateMap<CreateProductCommand, Product>().MaxDepth(64);
        CreateMap<UpdateProductCommand, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .MaxDepth(64);
    }
}
