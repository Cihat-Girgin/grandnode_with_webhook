﻿using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Domain.Catalog;
using Grand.Infrastructure.Mapper;

namespace Grand.Api.Infrastructure.Mapper.Profiles;

public class TierPriceProfile : Profile, IAutoMapperProfile
{
    public TierPriceProfile()
    {
        CreateMap<ProductTierPriceDto, TierPrice>();

        CreateMap<TierPrice, ProductTierPriceDto>();
    }

    public int Order => 1;
}