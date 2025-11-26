using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Для чтения пароля из appsettings
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
using MimeKit; // Для почты
using MailKit.Net.Smtp; // Для отправки

namespace online_courses.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IBaseStorage<UserDb> _userStorage;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config; // Добавили конфигурацию

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
                // 1. Проверка существования
                var users = await _userStorage.GetAllAsync();
                if (users.Any(x => x.Email == model.Email))
                {
                    return new BaseResponse<User>()
                    {
                        Description = "Пользователь с такой почтой уже есть",
                        StatusCode = StatusCode.UserAlreadyExists
                    };
                }

                // 2. Маппинг и Валидация
                var newUser = _mapper.Map<User>(model);
                var userValidator = new UserValidator();
                var validationResult = await userValidator.ValidateAsync(newUser);
                if (!validationResult.IsValid)
                {
                    return new BaseResponse<User>()
                    {
                        Description = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        StatusCode = StatusCode.InternalServerError
                    };
                }

                // 3. Генерируем код и отправляем письмо
                Random random = new Random();
                newUser.GeneratedCode = random.Next(10000, 99999).ToString(); // Код из 5 цифр

                await SendMessage(newUser.Email, "Код подтверждения", $"Ваш код: {newUser.GeneratedCode}");

                // 4. Хешируем пароль, но В БАЗУ ПОКА НЕ ПИШЕМ
                newUser.Password = HashPasswordHelper.HashPass(model.Password);

                return new BaseResponse<User>()
                {
                    Data = newUser, // Возвращаем пользователя с кодом, чтобы передать его на фронт
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

        // Новый метод подтверждения
        public async Task<BaseResponse<ClaimsIdentity>> ConfirmEmail(User user, string code)
        {
            try
            {
                // Проверяем код
                if (user.GeneratedCode != code)
                {
                    return new BaseResponse<ClaimsIdentity>()
                    {
                        Description = "Неверный код",
                        StatusCode = StatusCode.InternalServerError
                    };
                }

                // Если код верен - сохраняем в базу
                var userDb = _mapper.Map<UserDb>(user);
                await _userStorage.AddAsync(userDb);

                var result = Authenticate(user);

                return new BaseResponse<ClaimsIdentity>()
                {
                    Data = result,
                    Description = "Регистрация завершена",
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

        // Метод отправки письма
        private async Task SendMessage(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            string fromEmail = _config["Gmail:Email"];
            string fromPassword = _config["Gmail:Password"];

            emailMessage.From.Add(new MailboxAddress("Онлайн Курсы", fromEmail));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync(fromEmail, fromPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}