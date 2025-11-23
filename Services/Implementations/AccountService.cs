using AutoMapper;
using Microsoft.EntityFrameworkCore;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models;
using online_courses.Response;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using online_courses.Domain.Helpers;
using FluentValidation;
using online_courses.Domain.Validators;

namespace online_courses.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IMapper _mapper;

        public AccountService(IBaseStorage<UserDb> userStorage, IMapper mapper)
        {
            _userStorage = userStorage;
            _mapper = mapper;
        }

        public async Task<BaseResponse<ClaimsIdentity>> Register(RegisterViewModel model)
        {
            try
            {
                // 1. Проверяем, есть ли такой пользователь в базе
                var users = await _userStorage.GetAllAsync();
                var user = users.FirstOrDefault(x => x.Email == model.Email);

                if (user != null)
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Пользователь с такой почтой уже есть",
                        StatusCode = StatusCode.UserAlreadyExists
                    };
                }

                // 2. Создаем нового пользователя (превращаем ViewModel в Domain модель)
                var newUser = _mapper.Map<User>(model);

                // === 3. ВАЛИДАЦИЯ ДАННЫХ (ГЛАВА 16) ===
                var userValidator = new UserValidator();
                var validationResult = await userValidator.ValidateAsync(newUser);

                if (!validationResult.IsValid)
                {
                    // Собираем ошибки в одну строку
                    var errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = errorMessage,
                        StatusCode = StatusCode.InternalServerError
                    };
                }
                // ======================================

                // 4. Хешируем пароль
                newUser.Password = HashPasswordHelper.HashPass(model.Password);

                // 5. Превращаем Domain модель в Entity для базы
                var userDb = _mapper.Map<UserDb>(newUser);

                await _userStorage.AddAsync(userDb);

                // 6. Авторизуем сразу
                var result = Authenticate(newUser);

                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = result,
                    Description = "Объект добавился",
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClaimsIdentity>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model)
        {
            try
            {
                var users = await _userStorage.GetAllAsync();
                var user = users.FirstOrDefault(x => x.Email == model.Email);

                if (user == null)
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Пользователь не найден",
                        StatusCode = StatusCode.UserNotFound
                    };
                }

                // Проверка хеша пароля
                if (user.Password != HashPasswordHelper.HashPass(model.Password))
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Неверный пароль",
                        StatusCode = StatusCode.UserAlreadyExists
                    };
                }

                // Мапим обратно в доменную модель для авторизации
                var domainUser = _mapper.Map<User>(user);
                var result = Authenticate(domainUser);

                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = result,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ClaimsIdentity>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        private ClaimsIdentity Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };
            return new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        }
    }
}