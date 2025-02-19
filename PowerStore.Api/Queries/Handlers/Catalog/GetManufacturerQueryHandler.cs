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
    public class GetManufacturerQueryHandler : IRequestHandler<GetQuery<ManufacturerDto>, IMongoQueryable<ManufacturerDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetManufacturerQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }

        public Task<IMongoQueryable<ManufacturerDto>> Handle(GetQuery<ManufacturerDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(
                    _mongoDBContext.Database()
                    .GetCollection<ManufacturerDto>
                    (typeof(Domain.Catalog.Manufacturer).Name)
                    .AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database()
                    .GetCollection<ManufacturerDto>(typeof(Domain.Catalog.Manufacturer).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}
