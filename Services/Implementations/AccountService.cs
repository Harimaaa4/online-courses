using AutoMapper;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using online_courses.Domain;
using online_courses.Domain.Helpers;
using online_courses.Domain.Validators;
using online_courses.Entities;
using online_courses.Interfaces;
using online_courses.Models;
using online_courses.Response;
using online_courses.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace online_courses.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AccountService(IBaseStorage<UserDb> userStorage, IMapper mapper, IConfiguration config)
        {
            _userStorage = userStorage;
            _mapper = mapper;
            _config = config;
        }

        public async Task<BaseResponse<User>> Register(RegisterViewModel model)
        {
            try
            {
                var users = await _userStorage.GetAllAsync();
                if (users.Any(x => x.Email == model.Email))
                {
                    return new BaseResponse<User>()
                    {
                        Description = "Пользователь с такой почтой уже существует",
                        StatusCode = StatusCode.UserAlreadyExists
                    };
                }

                var newUser = _mapper.Map<User>(model);
                var validator = new UserValidator();
                var valResult = await validator.ValidateAsync(newUser);

                if (!valResult.IsValid)
                {
                    return new BaseResponse<User>()
                    {
                        Description = string.Join("; ", valResult.Errors.Select(x => x.ErrorMessage)),
                        StatusCode = StatusCode.InternalServerError
                    };
                }

                Random random = new Random();
                newUser.GeneratedCode = random.Next(10000, 99999).ToString();

                await SendMessage(newUser.Email, "Код подтверждения", $"Ваш код: {newUser.GeneratedCode}");

                // Хешируем пароль, но пока не сохраняем в БД (вернем на фронт для подтверждения)
                newUser.Password = HashPasswordHelper.HashPass(model.Password);

                return new BaseResponse<User>()
                {
                    Data = newUser,
                    Description = "Письмо отправлено",
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<User>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<BaseResponse<ClaimsIdentity>> ConfirmEmail(User user, string code)
        {
            try
            {
                if (user.GeneratedCode != code)
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Неверный код",
                        StatusCode = StatusCode.InternalServerError
                    };
                }

                // Пароль уже захеширован в методе Register (или пришел с фронта захешированным, если мы так сделали)
                // Но для надежности лучше убедиться, что мы сохраняем хеш.
                // В текущей логике Register возвращает уже захешированный пароль в response.Data, 
                // который потом передается в ConfirmEmailViewModel, и оттуда сюда.
                // Поэтому здесь user.Password уже должен быть хешем.

                user.Role = "User";
                var userDb = _mapper.Map<UserDb>(user);

                await _userStorage.AddAsync(userDb);

                var result = Authenticate(user);
                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = result,
                    Description = "Регистрация успешна",
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

                if (user == null || user.Password != HashPasswordHelper.HashPass(model.Password))
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Неверный логин или пароль",
                        StatusCode = StatusCode.UserNotFound
                    };
                }

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

        // МЕТОД ДЛЯ GOOGLE (ИСПРАВЛЕННЫЙ)
        public async Task<BaseResponse<ClaimsIdentity>> IsCreatedAccount(User model)
        {
            try
            {
                var users = await _userStorage.GetAllAsync();
                // userDb - это сущность из базы (UserDb)
                var userDb = users.FirstOrDefault(x => x.Email == model.Email);

                if (userDb == null)
                {
                    // Создаем нового (model - это User)
                    model.Password = HashPasswordHelper.HashPass(model.Password);
                    model.Role = "User";

                    var newUserDb = _mapper.Map<UserDb>(model);
                    await _userStorage.AddAsync(newUserDb);

                    var result = Authenticate(model);
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Data = result,
                        Description = "Регистрация через Google успешна",
                        StatusCode = StatusCode.OK
                    };
                }

                // Если пользователь есть, нужно превратить UserDb в User перед авторизацией
                // !!! ВОТ ЗДЕСЬ БЫЛА ОШИБКА !!!
                var domainUser = _mapper.Map<User>(userDb);
                var resultAuth = Authenticate(domainUser);

                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = resultAuth,
                    Description = "Вход через Google успешен",
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

        private async Task SendMessage(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                string fromEmail = _config["Gmail:Email"];
                string fromPassword = _config["Gmail:Password"];

                emailMessage.From.Add(new MailboxAddress("Онлайн Курсы", fromEmail));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, true);
                    await client.AuthenticateAsync(fromEmail, fromPassword);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка почты: " + ex.Message);
                throw;
            }
        }
    }
}