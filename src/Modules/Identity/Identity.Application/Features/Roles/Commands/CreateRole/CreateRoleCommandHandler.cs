using Identity.Application.Contracts.Persistence;
using Identity.Domain.Entities;
using Shared.CQRS;

namespace Identity.Application.Features.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateRoleResponse> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
        {
            var role = new Role
            {
                RoleName = command.RoleName,
                RoleDescription = command.RoleDescription
            };

            _roleRepository.AddRole(role);
            await _unitOfWork.SaveChangesAsync();

            return new CreateRoleResponse(role.Id, role.RoleName, role.RoleDescription); 
        }
    }
}
