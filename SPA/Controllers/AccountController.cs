﻿using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPA.Models.Dtos;
using SPA.Models.Entities;

namespace SPA.Controllers
{
    /// <summary>
    /// Account controller provides actions related to the user account state e.g logging in.
    /// the provided actions are registration and logging in.
    /// </summary>
    public class AccountController : ApiBaseController
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// Manager used with identity for creating users, deleting them and checking if they exist.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;
        /// <summary>
        /// Used with the login action to sign in users this will handle password hashing etc.
        /// </summary>
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Action to register a newly created user, we take in a RegisterDto with the properties provided for a new user creation
        /// then attempt to register this user through the user manager identity will provide us with any errors that caused the
        /// user not to be added e.g user name is already taken. Finally the user is returned with route as an AuthenticatedUserDto
        /// if created.
        /// </summary>
        /// <param name="registerDto">Verified registration properties through the RegisterDto from the POST request.</param>
        /// <returns>The mapped AuthenticatedUserDto from the user with minimal required properties.</returns>
        [Route("register")]
        [HttpPost]
        public async Task<ActionResult<AuthenticatedUserDto>> RegisterUserAsync(RegisterDto registerDto)
        {
            // Mapping registerDto properties to a new ApplicationUser then creating this new user in our database through identity.
            var userToRegister = _mapper.Map<ApplicationUser>(registerDto);

            if (userToRegister == null)
            {
                return BadRequest("Failed to create user from provided details.");
            }

            var userCreationResult = await _userManager.CreateAsync(userToRegister, registerDto.Password);

            if (!userCreationResult.Succeeded)
            {
                // Returning all errors in an easy to read format e.g user name already taken or email.
                var errorsResponse = new StringBuilder();
                foreach (var error in userCreationResult.Errors)
                {
                    errorsResponse.Append("\n");
                    errorsResponse.Append(error.Description);
                }
                return BadRequest($"Failed to register for the following reasons: {errorsResponse}");
            }

            // Mapping to a AuthenticatedUserDto so we only send back the required properties.
            var loggedInUser = _mapper.Map<AuthenticatedUserDto>(userToRegister);

            // TODO add route of getting user (once this action and controller is created).
            return CreatedAtRoute("", loggedInUser);
        }

        /// <summary>
        /// Action to login a user with the provided username and password. The sign in manager through identity is then
        /// used to try to verify the password with the user found with the provided username in the database.
        /// </summary>
        /// <param name="loginDto">LoginDto that receives the validated properties sent up in the POST request</param>
        /// <returns> Mapped AuthenticatedUserDto if login is valid otherwise Unauthorized response.</returns>
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult<AuthenticatedUserDto>> LoginAsync(LoginDto loginDto)
        {
            const string unauthorizedResponse = "Invalid username or password!";

            var userToSignIn = await
                _userManager.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);

            if (userToSignIn == null)
            {
                return Unauthorized(unauthorizedResponse);
            }

            var loginResult = await _signInManager.PasswordSignInAsync(userToSignIn, loginDto.Password, false, false);

            if (loginResult.Succeeded)
            {
                var loggedInUser = _mapper.Map<AuthenticatedUserDto>(userToSignIn);
                return Ok(loggedInUser);
            }

            return Unauthorized(unauthorizedResponse);
        }
    }
}