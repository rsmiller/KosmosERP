using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Prometheus.Database.Models;
using Prometheus.Models.Helpers;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Module;
using Prometheus.Database;
using Microsoft.EntityFrameworkCore;
using Prometheus.Api.Models.Module.User.Dto;
using Prometheus.Api.Models.Module.User.Command.Create;
using Prometheus.Api.Models.Module.User.Command.Delete;
using Prometheus.Api.Models.Module.User.Command.Edit;
using Prometheus.Api.Models.Module.User.Command.Find;

namespace Prometheus.Api.Modules;

public partial interface IUserModule : IERPModule<Prometheus.Database.Models.User, UserDto, UserListDto, UserCreateCommand, UserEditCommand, UserDeleteCommand, UserFindCommand>, IBaseERPModule
{
    Task<Response<AuthenticatedUserDto>> Authenticate(string email, string password);
    Task<Response<UserPermissionsSet>> GetPermissionSet(int? user_id);
    Task<Response<UserDto>> GetBySession(string session_id);
    Task<Response<List<UserDto>>> GetUsersByDepartment(int department_id);
    Task<Response<bool>> AssignUserRole(AssignUserRoleCommand command);
    Task<Response<List<RoleDto>>> GetRoles();
}

public partial class UserModule : BaseERPModule, IUserModule
{
    public override Guid ModuleIdentifier => Guid.Parse("b8b0d255-3901-4007-b9c7-b0678f89c955");
    public override string ModuleName => "Users";

    private IBaseERPContext _IContext;

    public UserModule(IBaseERPContext context) : base (context)
    {
        _IContext = context;
    }

    public override void SeedPermissions()
    {
        var admin_role = _IContext.Roles.Any(m => m.name == "Administrators");
        var read_user_permission = _IContext.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_user");
        var create_user_permission = _IContext.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_user");
        var edit_user_permission = _IContext.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_user");
        var system_user = _IContext.Users.Any(m => m.username == "system");

        if(admin_role == false)
        {
            _IContext.Roles.Add(new Role()
            {
                name = "Administrators",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _IContext.SaveChanges();
        }

        if(read_user_permission == false)
        {
            _IContext.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read User",
                internal_permission_name = "read_user",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _IContext.SaveChanges();

            var role_id = _IContext.Roles.Where(m => m.name == "Administrators").Select(m => m.id).Single();
            var read_user_perm_id = _IContext.ModulePermissions.Where(m => m.internal_permission_name == "read_user").Select(m => m.id).Single();

            _IContext.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_user_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _IContext.SaveChanges();
        }

        if(create_user_permission == false)
        {
            _IContext.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create User",
                internal_permission_name = "create_user",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _IContext.SaveChanges();

            var role_id = _IContext.Roles.Where(m => m.name == "Administrators").Select(m => m.id).Single();
            var create_user_perm_id = _IContext.ModulePermissions.Where(m => m.internal_permission_name == "create_user").Select(m => m.id).Single();

            _IContext.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_user_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _IContext.SaveChanges();
        }

        if (edit_user_permission == false)
        {
            _IContext.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit User",
                internal_permission_name = "edit_user",
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _IContext.SaveChanges();

            var role_id = _IContext.Roles.Where(m => m.name == "Administrators").Select(m => m.id).Single();
            var edit_user_perm_id = _IContext.ModulePermissions.Where(m => m.internal_permission_name == "edit_user").Select(m => m.id).Single();

            _IContext.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_user_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _IContext.SaveChanges();
        }

        if(system_user == false)
        {
            string new_password = Guid.NewGuid().ToString().Substring(0, 13);

            var password = this.GeneratePasword(new_password);

            Console.WriteLine("Administrator user generated with password: " + new_password);

            _IContext.Users.Add(new User()
            {
                username = "system",
                employee_number = "1",
                department = 0,
                first_name = "System",
                last_name = "User",
                password = password.HashedPassword,
                password_salt = password.Salt,
                is_admin = true,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _IContext.SaveChanges();

            var role_id = _IContext.Roles.Where(m => m.name == "Administrators").Select(m => m.id).Single();
            var user_id = _IContext.Users.Where(m => m.username == "system").Select(m => m.id).Single();

            _IContext.UserRoles.Add(new UserRole()
            {
                role_id = role_id,
                user_id = user_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _IContext.SaveChanges();
        }
    }

    public async Task<Response<List<RoleDto>>> GetRoles()
    {
        Response<List<RoleDto>> response = new Response<List<RoleDto>>();
        response.Data = new List<RoleDto>();

        try
        {
            var roles = await _IContext.Roles.Where(m => m.is_deleted == false).ToListAsync();
            
            foreach(var role in roles)
            {
                var dto = new RoleDto()
                { 
                    name = role.name,
                    role_id = role.id
                };

                var role_permissions = await _IContext.RolePermissions.Where(m => m.role_id == role.id).ToListAsync();

                foreach (var role_permission in role.role_permissions)
                {
                    var module_perm_dto = new ModuleRolePermissionDto()
                    {
                        module_id = role_permission.module_permission.module_id,
                        module_name = role_permission.module_permission.module_name,
                        module_permission_id = role_permission.module_permission.id,
                        permission_name = role_permission.module_permission.permission_name,
                        read = role_permission.module_permission.read,
                        write = role_permission.module_permission.write,
                        edit = role_permission.module_permission.edit,
                        delete = role_permission.module_permission.delete,
                        requires_admin = role_permission.module_permission.requires_admin,
                        requires_management = role_permission.module_permission.requires_management,
                        requires_guest = role_permission.module_permission.requires_guest,
                        is_active = role_permission.module_permission.is_active
                    };

                    dto.module_permission.Add(module_perm_dto);
                }

                response.Data.Add(dto);
            }

        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "GetRoles", ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<UserPermissionsSet>> GetPermissionSet(int user_id)
    {
        Response<UserPermissionsSet> response = new Response<UserPermissionsSet>();

        try
        {
            response.Data = new UserPermissionsSet()
            {
                user_id = user_id,
                permissions = new List<ModulePermission>(),
            };


            var results = await (from ur in _IContext.UserRoles
                            join r in _IContext.Roles on ur.role_id equals r.id
                            join rp in _IContext.RolePermissions on r.id equals rp.role_id
                            where ur.user_id == user_id
                            select new { role_id = r.id, permission = rp.module_permission }).ToListAsync();


            foreach (var result in results)
                response.Data.permissions.Add(result.permission);
        }
        catch (Exception ex)
        {
            await LogError(90, this.GetType().Name, "GetPermissionSet", ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<UserDto>> GetBySession(string session_id)
    {
        Response<UserDto> response = new Response<UserDto>();

        try
        {
            var result = await _IContext.UserSessionStates.FirstOrDefaultAsync(m => m.session_id == session_id);

            if (result == null || result.session_expires <= DateTime.Now)
            {
                response.SetException("User session expired or does not exist", ResultCode.NotFound);
            }
            else
            {
                var user = await this.GetDto(result.user_id);

                response.ResultCode = ResultCode.Okay;
                response.Data = user.Data;
            }


        }
        catch (Exception ex)
        {
            await LogError(90, this.GetType().Name, "GetBySession", ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<List<UserDto>>> GetUsersByDepartment(int department_id)
    {
        Response<List<UserDto>> response = new Response<List<UserDto>>();
        response.Data = new List<UserDto>();

        try
        {
            var results = await _IContext.Users.Where(m => m.department == department_id
                                                        && m.is_deleted == false).ToListAsync();

            foreach (var user in results)
                response.Data.Add(await this.MapToDto(user));

        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "GetUsersByDepartment", ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<AuthenticatedUserDto>> Authenticate(string username, string password)
    {
        try
        {
            var result = await _IContext.Users.SingleOrDefaultAsync(m => m.username.ToLower() == username);

            if (result != null && result.is_deleted)
                return new Response<AuthenticatedUserDto>("User is deactivated", ResultCode.InvalidPermission);

            if (result != null)
            {
                var hashedPassword = HashPasword(password, result.password_salt);

                if (hashedPassword == result.password)
                {
                    var userDto = await this.MapToDto(result);
                    var sessionState = await this.FindCreateOrUpdateUserSession(userDto.id);

                    var dto = new AuthenticatedUserDto()
                    {
                        id = result.id,
                        authenticated = true,
                        user = userDto,
                        session = sessionState
                    };

                    return new Response<AuthenticatedUserDto>(dto);
                }
            }

            return new Response<AuthenticatedUserDto>("Credentials could not be authenticated", ResultCode.Invalid);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Authenticate", ex);

            return new Response<AuthenticatedUserDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<UserDto>> GetDto(int object_id)
    {
        Response<UserDto> response = new Response<UserDto>();

        var result = await _IContext.Users.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("User not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<bool>> AssignUserRole(AssignUserRoleCommand commandModel)
    {
        Response<bool> response = new Response<bool>();

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<bool>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_user", write: true);
        if (!permission_result)
            return new Response<bool>("Invalid permission", ResultCode.InvalidPermission);

        var user = await _IContext.Users.SingleOrDefaultAsync(m => m.id == commandModel.user_id);
        if (user == null)
            return new Response<bool>("User not found", ResultCode.NotFound);

        var permission = _IContext.Roles.SingleOrDefault(m => m.id == commandModel.role_id);
        if (permission == null)
            return new Response<bool>("Role not found", ResultCode.NotFound);

        var user_role = _IContext.UserRoles.SingleOrDefault(m => m.user_id == commandModel.user_id && m.role_id == commandModel.role_id);
        if (permission == null)
            return new Response<bool>("User already has role", ResultCode.AlreadyExists);

        await _IContext.UserRoles.AddAsync(new UserRole()
        {
            user_id = commandModel.user_id,
            role_id = commandModel.role_id,
            created_by = commandModel.user_id,
            updated_by = commandModel.user_id,
            created_on = DateTime.Now,
            updated_on = DateTime.Now,
        });

        await _IContext.SaveChangesAsync();

        return new Response<bool>(true);
    }

    public async Task<Response<UserDto>> Create(UserCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<UserDto>("Null", ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<UserDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_user", write: true);
        if (!permission_result)
            return new Response<UserDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var alreadyExists = await _IContext.Users.FirstOrDefaultAsync(m => m.username.ToLower() == commandModel.username.ToLower());

            if (alreadyExists != null)
                return new Response<UserDto>("Username already in use", ResultCode.AlreadyExists);


            var item = this.MapForCreate(commandModel);

            await _IContext.Users.AddAsync(item);
            await _IContext.SaveChangesAsync();

            var dto = await this.MapToDto(item);

            return new Response<UserDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Create", ex);
            return new Response<UserDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<UserDto>> Edit(UserEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<UserDto>("Null", ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<UserDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await _IContext.Users.SingleOrDefaultAsync(m => m.id == commandModel.id);
        if (existingEntity == null)
            return new Response<UserDto>("User not found", ResultCode.NotFound);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_user", edit: true);
        if (!permission_result)
            return new Response<UserDto>("Invalid permission", ResultCode.InvalidPermission);


        var oldUneditedEntity = existingEntity;

        //Strings
        var hashedPass = this.HashPasword(commandModel.password, existingEntity.password_salt);
        // UI will send 1234 as an unset password
        if (!String.IsNullOrEmpty(commandModel.password) && commandModel.password != "1234" && existingEntity.password != hashedPass)
        {
            // Generate new password
            var passwordInfo = GeneratePasword(commandModel.password);
            existingEntity.password = passwordInfo.HashedPassword;
            existingEntity.password_salt = passwordInfo.Salt;
        }


        if (!String.IsNullOrEmpty(commandModel.first_name) && existingEntity.first_name != commandModel.first_name)
            existingEntity.first_name = commandModel.first_name;
        if (!String.IsNullOrEmpty(commandModel.last_name) && existingEntity.last_name != commandModel.last_name)
            existingEntity.last_name = commandModel.last_name;
        if (!String.IsNullOrEmpty(commandModel.email) && existingEntity.email != commandModel.email)
            existingEntity.email = commandModel.email; // There is no email in emp

        if (!String.IsNullOrEmpty(commandModel.employee_number) && existingEntity.employee_number != commandModel.employee_number)
            existingEntity.employee_number = commandModel.employee_number;


        //Numbers
        if (commandModel.department.HasValue && existingEntity.department != commandModel.department)
            existingEntity.department = commandModel.department;

        if (commandModel.is_external_user.HasValue && existingEntity.is_external_user != commandModel.is_external_user)
            existingEntity.is_external_user = commandModel.is_external_user.Value;

        if (commandModel.is_deleted.HasValue && existingEntity.is_deleted != commandModel.is_deleted)
        {
            if(commandModel.is_deleted == true && existingEntity.is_deleted == false)
            {
                existingEntity.deleted_by = commandModel.calling_user_id;
                existingEntity.deleted_on = DateTime.Now;
            }

            if (commandModel.is_deleted == false && existingEntity.is_deleted == true)
            {
                existingEntity.deleted_by = null;
                existingEntity.deleted_on = null;
            }

            existingEntity.is_deleted = commandModel.is_deleted.Value;
        }
            

        if (commandModel.is_admin.HasValue && commandModel.is_admin != existingEntity.is_admin)
            existingEntity.is_admin = commandModel.is_admin.Value;

        if (commandModel.is_guest.HasValue && commandModel.is_guest != existingEntity.is_guest)
            existingEntity.is_guest = commandModel.is_guest.Value;

        if (commandModel.is_management.HasValue && commandModel.is_management != existingEntity.is_management)
            existingEntity.is_management = commandModel.is_management.Value;

        try
        {
            existingEntity.updated_on = DateTime.Now;
            existingEntity.updated_by = commandModel.calling_user_id;

            _IContext.Users.Update(existingEntity);
            await _IContext.SaveChangesAsync();

            return new Response<UserDto>(await this.MapToDto(existingEntity));
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Edit", ex);
            return new Response<UserDto>(ex.Message, ResultCode.Error);
        }
    }

    public Task<Response<UserDto>> Delete(UserDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public async Task<PagingResult<UserListDto>> Find(PagingSortingParameters parameters, UserFindCommand commandModel)
    {
        PagingResult<UserListDto> response = new PagingResult<UserListDto>();

        try
        {
            var permission_result = await base.HasPermission(commandModel.calling_user_id, "read_user", read: true);
            if (!permission_result)
                return new PagingResult<UserListDto>("Invalid permission", ResultCode.InvalidPermission);

            List<UserListDto> dtos = new List<UserListDto>();

            var results = _IContext.Users.Where(m => m.is_deleted == false &&
                                        (m.first_name.Contains(commandModel.wildcard)
                                        || m.last_name.Contains(commandModel.wildcard)
                                        || m.employee_number.Contains(commandModel.wildcard)
                                        || m.username.Contains(commandModel.wildcard)
                                        || m.email.Contains(commandModel.wildcard)));


            var sortedResults = await results.SortAndPageBy(parameters).ToListAsync();

            foreach (var result in results)
                dtos.Add(await this.MapToListDto(result));

            response.Data = dtos;
            response.TotalResultCount = results.Count();
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Find", ex);

            response.TotalResultCount = 0;
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    /// <summary>
    /// If end user wishes to perform a global search on users this method will be called
    /// </summary>
    /// <param name="parameters">Limit and sort parameters</param>
    /// <param name="wildcard">Can be name, employee number, or email</param>
    /// <param name="callingUserPermissions">The user permissions who is calling this method</param>
    /// <returns></returns>
    public async Task<Response<List<UserListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        Response<List<UserListDto>> response = new Response<List<UserListDto>>();
        response.Data = new List<UserListDto>();

        try
        {
            var results = _IContext.Users.Where(m => m.is_deleted == false &&
                                        (m.first_name.Contains(wildcard)
                                        || m.last_name.Contains(wildcard)
                                        || m.employee_number.Contains(wildcard)
                                        || m.username.Contains(wildcard)
                                        || m.email.Contains(wildcard)));


            var sortedResults = await results.SortAndPageBy(parameters).ToListAsync();

            foreach (var result in results)
                response.Data.Add(await this.MapToListDto(result));

        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, "Find", ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<UserDto> MapToDto(Prometheus.Database.Models.User databaseModel)
    {
        return new UserDto
        {
            id = databaseModel.id,
            email = databaseModel.email,
            first_name = databaseModel.first_name,
            last_name = databaseModel.last_name,
            username = databaseModel.username,
            employee_number = databaseModel.employee_number,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            department = databaseModel.department,
            is_admin = databaseModel.is_admin,
            is_management = databaseModel.is_management,
            is_external_user = databaseModel.is_external_user,
            is_guest = databaseModel.is_guest,

        };
    }

    public async Task<UserListDto> MapToListDto(User databaseModel)
    {
        return new UserListDto
        {
            id = databaseModel.id,
            email = databaseModel.email,
            first_name = databaseModel.first_name,
            last_name = databaseModel.last_name,
            username = databaseModel.username,
            employee_number = databaseModel.employee_number,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            department = databaseModel.department,
            is_admin = databaseModel.is_admin,
            is_management = databaseModel.is_management,
            is_external_user = databaseModel.is_external_user,
            is_guest = databaseModel.is_guest,

        };
    }

    public Prometheus.Database.Models.User MapToDatabaseModel(UserDto dtoModel)
    {
        throw new NotImplementedException();
    }

    private Prometheus.Database.Models.User MapForCreate(UserCreateCommand createCommandModel)
    {
        var now = DateTime.Now;

        var user = new Prometheus.Database.Models.User()
        {
            email = createCommandModel.email,
            first_name = createCommandModel.first_name,
            last_name = createCommandModel.last_name,
            username = createCommandModel.username,
            employee_number = createCommandModel.employee_number,
            is_deleted = createCommandModel.is_deleted,
            created_by = createCommandModel.calling_user_id,
            updated_by = createCommandModel.calling_user_id,
            department = createCommandModel.department,
            is_admin = createCommandModel.is_admin,
            is_management = createCommandModel.is_management,
            is_external_user = createCommandModel.is_external_user,
            is_guest = createCommandModel.is_guest,
            created_on = now,
            updated_on = now

        };

        var passwordInfo = GeneratePasword(createCommandModel.password);

        user.password = passwordInfo.HashedPassword;
        user.password_salt = passwordInfo.Salt;

        return user;
    }

    /// <summary>
    /// This method creates a user session or updates the expiration time for a user session
    /// </summary>
    /// <param name="user_id">The users internal id</param>
    /// <returns>UserSessionState</returns>
    private async Task<UserSessionState> FindCreateOrUpdateUserSession(int user_id)
    {
        var session = await _IContext.UserSessionStates.FirstOrDefaultAsync(m => m.user_id == user_id);

        if (session != null)
        {
            session.session_expires = session.session_expires.AddHours(6);
            _IContext.UserSessionStates.Update(session);
            await _IContext.SaveChangesAsync();
        }
        else
        {
            session = new UserSessionState()
            {
                user_id = user_id,
                created_on = DateTime.Now,
                session_id = Guid.NewGuid().ToString(),
                session_expires = DateTime.Now.AddHours(6),
            };

            await _IContext.UserSessionStates.AddAsync(session);
            await _IContext.SaveChangesAsync();
        }

        return session;
    }

    private string HashPasword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: saltBytes,
        prf: KeyDerivationPrf.HMACSHA1,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));

        return hashedPassword;
    }

    private UserPassword GeneratePasword(string password)
    {
        var newSalt = GenerateSalt();
        var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: password,
        salt: newSalt,
        prf: KeyDerivationPrf.HMACSHA1,
        iterationCount: 10000,
        numBytesRequested: 256 / 8));

        return new UserPassword()
        {
            Salt = Convert.ToBase64String(newSalt),
            HashedPassword = hashedPassword
        };
    }

    private byte[] GenerateSalt()
    {
        var bytes = new byte[128 / 8];
        RandomNumberGenerator.Fill(bytes);

        return bytes;
    }

    public string ComparePassword(byte[] bytesToHash, byte[] salt)
    {
        var byteResult = new Rfc2898DeriveBytes(bytesToHash, salt, 10000);
        return Convert.ToBase64String(byteResult.GetBytes(24));
    }

    public Prometheus.Database.Models.User? Get(int object_id)
    {
        return _IContext.Users.Single(m => m.id == object_id);
    }

    public async Task<Prometheus.Database.Models.User?> GetAsync(int object_id)
    {
        return await _IContext.Users.SingleAsync(m => m.id == object_id);
    }

    private class UserPassword
    {
        public string HashedPassword { get; set; } = "";
        public string Salt { get; set; } = "";
    }
}
