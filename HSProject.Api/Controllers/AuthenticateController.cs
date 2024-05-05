
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HSProject.Api.Authentication;

namespace HSProject.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase {

    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IConfiguration configuration;
    private readonly ILogger<AuthenticateController> logger;

    public AuthenticateController(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger<AuthenticateController> logger) {


        this.userManager = userManager;
        this.roleManager = roleManager;
        this.configuration = configuration;
        this.logger = logger;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model) {
        var user = await userManager.FindByNameAsync(model.Username);
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) {
            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id)
            };

            foreach (var userRole in userRoles) {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            logger.LogInformation($"{user.UserName} logged in successfully");

            return Ok(new {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
        logger.LogError("Login failure");
        return Unauthorized();
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model) {
        var userExists = await userManager.FindByNameAsync(model.Username);
        if (userExists != null) {

            logger.LogError($"Registration failed. User {userExists.UserName} already exists");

            return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "User already exists!" });
        }


        ApplicationUser user = new() {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        IdentityResult result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded) {

            List<IdentityError> errorList = result.Errors.ToList();
            string errors = string.Join(", ", errorList.Select(e => e.Description));

            logger.LogError($"Error registering  {model.Username}. Errors: {errors}");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                });
        }
        await userManager.AddToRoleAsync(user, UserRoles.User);
        logger.LogInformation($"Registered {user.UserName} with email {user.Email}");
        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model) {
        var userExists = await userManager.FindByNameAsync(model.Username);

        if (userExists != null) {
            logger.LogError($"Registration failed. User {userExists.UserName} already exists");

            return StatusCode(StatusCodes.Status500InternalServerError,
                              new Response { Status = "Error", Message = "User already exists!" });
        }

        ApplicationUser user = new() {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded) {

            List<IdentityError> errorList = result.Errors.ToList();
            string errors = string.Join(", ", errorList.Select(e => e.Description));

            logger.LogError($"Error registering admin {model.Username}. Errors: {errors}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new Response {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                });
        }

        await userManager.AddToRoleAsync(user, UserRoles.Admin);
        logger.LogInformation($"Registered admin {user.UserName} with email {user.Email}");
        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }

    [HttpPost]
    [Route("init")]
    public async Task<IActionResult> Init() {

        logger.LogWarning($"Initializing service user");

        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if (!admins.Any()) {
            ApplicationUser user = new() {
                Email = "admin@labelservice.dmu",
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = "admin"
            };

            string password = PasswordGenerator.Generate(12);

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) {

                List<IdentityError> errorList = result.Errors.ToList();
                string errors = string.Join(", ", errorList.Select(e => e.Description));

                logger.LogError($"Failed to create first user. Errors: {errors}");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response {
                        Status = "Error",
                        Message = "User creation failed! Please try again."
                    });
            }


            if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await roleManager.RoleExistsAsync(UserRoles.Admin)) {
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            logger.LogInformation("Successfully initialized. Default admin was created");

            return Ok(new {
                Name = user.UserName,
                user.Email,
                password
            });
        } else {
            logger.LogError("Attempting to init more than once");
            return Forbid();
        }
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel changePasswordModel) {

        ApplicationUser? user = await userManager.GetUserAsync(User);
        if (user == null) {
            logger.LogError("Password change failed, user not found");
            return Forbid();
        }
        IdentityResult result = await userManager.ChangePasswordAsync(
            user,
            changePasswordModel.CurrentPassword,
            changePasswordModel.NewPassword);
        if (result.Succeeded) {
            logger.LogInformation($"User {user.UserName} successfully changed his password");
            return Ok("Password changed successfully.");
        }
        List<IdentityError> errorList = result.Errors.ToList();
        string errors = string.Join(", ", errorList.Select(e => e.Description));
        logger.LogError($"Failed to change password for user {user.UserName}. Errors: {errors}");
        return BadRequest(result.Errors);
    }

    [HttpPut("change-email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailModel changeEmailModel) {

        ApplicationUser? user = await userManager.GetUserAsync(User);
        if (user == null) {
            logger.LogError("Email change failed, user not found");
            return Forbid();
        }
        user.Email = changeEmailModel.NewEmail;
        IdentityResult result = await userManager.UpdateAsync(user);
        if (result.Succeeded) {
            logger.LogInformation($"User {user.UserName} successfully changed his email");
            return Ok("Email changed successfully.");
        }
        List<IdentityError> errorList = result.Errors.ToList();
        string errors = string.Join(", ", errorList.Select(e => e.Description));
        logger.LogError($"Failed to change email for user {user.UserName}. Errors: {errors}");
        return BadRequest(result.Errors);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ShowUserInfo() {

        ApplicationUser? currentUser = await userManager.GetUserAsync(User);
        string userName = currentUser?.UserName ?? "unknown";
        if (User.IsInRole("Admin")) {
            List<ApplicationUser> users = userManager.Users.ToList();
            List<UserInfoDTO> usersWithRoles = new();

            IEnumerable<Task> tasks = users.Select(async user => {
                IList<string> roles = await userManager.GetRolesAsync(user);
                UserInfoDTO userDto = new() {
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                };
                usersWithRoles.Add(userDto);
            });

            await Task.WhenAll(tasks);

            logger.LogInformation($"Showing user info to admin {userName}");
            return Ok(usersWithRoles);
        }


        if (currentUser == null) {
            logger.LogError($"Show user info failed, user {userName} not found");
            return Forbid();
        } else {
            logger.LogInformation($"Showing user info to user {userName}");
            UserInfoDTO userDto = new() {
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                Role = "User"
            };
            return Ok(userDto);
        }
    }


    [HttpDelete("delete")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteUserModel model) {

        if (model?.UserName == null) {
            logger.LogError("Failed to delete account, user name was not specified");
            return BadRequest("User name not specified");
        }

        var user = await userManager.FindByNameAsync(model.UserName);
        if (user != null) {
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded) {
                await userManager.UpdateSecurityStampAsync(user);
                return Ok($"Account {user.UserName} deleted successfully.");
            }
            List<IdentityError> errorList = result.Errors.ToList();
            string errors = string.Join(", ", errorList.Select(e => e.Description));
            logger.LogError($"Failed to delete user {user.UserName}. Errors: {errors}");
            return BadRequest(result.Errors);
        }
        logger.LogError("Failed to delete account, user was not found");
        return NotFound("User not found.");
    }
}