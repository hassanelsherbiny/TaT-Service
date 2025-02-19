﻿using PowerStore.Api.DTOs.Catalog;
using PowerStore.Api.Queries.Models.Common;
using PowerStore.Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerStore.Api.Queries.Handlers.Common
{
    public class GetSpecificationAttributeQueryHandler : IRequestHandler<GetQuery<SpecificationAttributeDto>, IMongoQueryable<SpecificationAttributeDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetSpecificationAttributeQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<SpecificationAttributeDto>> Handle(GetQuery<SpecificationAttributeDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(
                    _mongoDBContext.Database()
                    .GetCollection<SpecificationAttributeDto>
                    (typeof(Domain.Catalog.SpecificationAttribute).Name)
                    .AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database()
                    .GetCollection<SpecificationAttributeDto>(typeof(Domain.Catalog.SpecificationAttribute).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}
