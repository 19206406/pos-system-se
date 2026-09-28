using Identity.Data.Entities;
using Identity.Data.Repositories.Interfaces;
using Identity.DTOs.Request;
using Identity.DTOs.Responses;

namespace Identity.UseCases.Users
{
    public class RegisterUser
    {
        private readonly IUserRepository _repository;

        public RegisterUser(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<RegisterUserResponseDTO> Execute(RegisterUserRequestDTO request)
        {
            // validation with FluentValidation 

            User newUser = new User
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                JobTitle = request.JobTitle,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.CreateUser(newUser);

            return new RegisterUserResponseDTO(newUser.Id); 
        }
    }
}
