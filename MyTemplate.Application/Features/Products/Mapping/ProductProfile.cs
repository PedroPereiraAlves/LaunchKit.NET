using AutoMapper;
using MyTemplate.Application.DTOs;
using MyTemplate.Application.Features.Products.Commands;
using MyTemplate.Domain.Entities;

namespace MyTemplate.Application.Features.Products.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<CreateProductCommand, Product>();
    }
}
