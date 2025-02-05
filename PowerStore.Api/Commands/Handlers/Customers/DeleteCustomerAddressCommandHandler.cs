﻿using PowerStore.Api.Commands.Models.Customers;
using PowerStore.Services.Customers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerStore.Api.Commands.Handlers.Customers
{
    public class DeleteCustomerAddressCommandHandler : IRequestHandler<DeleteCustomerAddressCommand, bool>
    {
        private readonly ICustomerService _customerService;

        public DeleteCustomerAddressCommandHandler(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<bool> Handle(DeleteCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerService.GetCustomerById(request.Customer.Id);
            if (customer != null)
            {
                var address = customer.Addresses.FirstOrDefault(x => x.Id == request.Address.Id);
                if (address != null)
                {
                    address.CustomerId = request.Customer.Id;
                    await _customerService.DeleteAddress(address);
                }
            }
            return true;
        }
    }
}
