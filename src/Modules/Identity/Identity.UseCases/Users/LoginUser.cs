using Identity.Data.Entities;
using Identity.Data.Repositories.Interfaces;
using Identity.DTOs.Request;
using Identity.DTOs.Responses;

namespace Identity.UseCases.Users
{
    public class LoginUser
    {
        private readonly IUserRepository _repository;

        public LoginUser(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<LoginUserResponseDTO> Execute(LoginUserRequestDTO request)
        {
            User user = await _repository.GetUserByEmail(request.Email);

            if (user is null)
                throw new Exception();

            if (user.HashPassword is null)
                throw new Exception();

            return new LoginUserResponseDTO(true); 
        }
    }
}
