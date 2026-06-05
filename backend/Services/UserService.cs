using ProjectBrain.Api.Auth;
using ProjectBrain.Api.Data;
using ProjectBrain.Api.Dtos;
using ProjectBrain.Api.Entities;

namespace ProjectBrain.Api.Services;

public class UserService : IUserService
{
    private readonly DbContext _dbContext;
    private readonly JwtHelper _jwtHelper;

    public UserService(DbContext dbContext, JwtHelper jwtHelper)
    {
        _dbContext = dbContext;
        _jwtHelper = jwtHelper;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Db.Queryable<SysUser>()
            .FirstAsync(x => x.UserName == request.UserName && !x.IsDeleted);

        if (user is null || !user.IsEnabled)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return new LoginResponse
        {
            Token = _jwtHelper.GenerateToken(user),
            User = new UserInfoDto
            {
                Id = user.Id,
                UserName = user.UserName,
                NickName = user.NickName
            }
        };
    }

    public async Task<UserInfoDto?> GetByIdAsync(long id)
    {
        var user = await _dbContext.Db.Queryable<SysUser>()
            .FirstAsync(x => x.Id == id && !x.IsDeleted);

        if (user is null)
        {
            return null;
        }

        return new UserInfoDto
        {
            Id = user.Id,
            UserName = user.UserName,
            NickName = user.NickName
        };
    }
}
