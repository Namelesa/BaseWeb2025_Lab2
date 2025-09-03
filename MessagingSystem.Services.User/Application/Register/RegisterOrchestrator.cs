using AutoMapper;
using FluentValidation;
using MessagingSystem.Services.User.Application.Register.Dto;
using MessagingSystem.Services.User.Core.User;
using MessagingSystem.Services.User.Infrastructure.HasherInfo;
using MessagingSystem.Services.User.Infrastructure.PasswordHasher;

namespace MessagingSystem.Services.User.Application.Register;

public class RegisterOrchestrator(
    IUserRepository userRepository, 
    IMapper mapper, 
    IValidator<RegisterDto> validator,
    IHasherPassword hasherPassword,
    IHasher hasher) : IRegisterOrchestrator
{
    public async Task<OperationResult<string>> RegisterUserAsync(RegisterDto registerDto)
    {
        var validationResult = await validator.ValidateAsync(registerDto);
        if (!validationResult.IsValid) 
            return OperationResult<string>.Fail(string.Join("; ", validationResult.Errors));
        
        registerDto.Password = hasherPassword.Hash(registerDto.Password);
            
        var hashLogin = hasher.Hash(registerDto.Login);
        var hashEmail = hasher.Hash(registerDto.Email);
        var hashNickName = hasher.Hash(registerDto.NickName);
            
        var user = mapper.Map<Core.User.User>(registerDto);
        user.SetHashes(hashLogin, hashEmail, hashNickName);
        
        try
        {
            await userRepository.AddUserAsync(user);
            return OperationResult<string>.Ok("User registered successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return OperationResult<string>.Fail($"User can not be added {e.Message}");
        }
    }
}