﻿using Microsoft.AspNetCore.Identity;

namespace MMSA.BLL.Services.Implementation.Auth;

public sealed class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Error(nameof(DefaultError), "Сталася помилка. Спробуйте ще раз.");

    public override IdentityError DuplicateEmail(string email) =>
        Error(nameof(DuplicateEmail), $"Користувач з email «{email}» уже зареєстрований.");

    public override IdentityError DuplicateUserName(string userName) =>
        Error(nameof(DuplicateUserName), $"Користувач з email «{userName}» уже зареєстрований.");

    public override IdentityError InvalidEmail(string? email) =>
        Error(nameof(InvalidEmail), $"Email «{email}» має неправильний формат.");

    public override IdentityError InvalidUserName(string? userName) =>
        Error(nameof(InvalidUserName), $"Email «{userName}» містить недопустимі символи.");

    public override IdentityError PasswordTooShort(int length) =>
        Error(nameof(PasswordTooShort), $"Пароль має містити щонайменше {length} символів.");

    public override IdentityError PasswordRequiresDigit() =>
        Error(nameof(PasswordRequiresDigit), "Пароль має містити хоча б одну цифру.");

    public override IdentityError PasswordRequiresLower() =>
        Error(nameof(PasswordRequiresLower), "Пароль має містити хоча б одну малу латинську літеру.");

    public override IdentityError PasswordRequiresUpper() =>
        Error(nameof(PasswordRequiresUpper), "Пароль має містити хоча б одну велику латинську літеру.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric), "Пароль має містити хоча б один спецсимвол.");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Error(nameof(PasswordRequiresUniqueChars), $"Пароль має містити щонайменше {uniqueChars} різних символів.");

    public override IdentityError PasswordMismatch() =>
        Error(nameof(PasswordMismatch), "Неправильний пароль.");

    public override IdentityError UserAlreadyInRole(string role) =>
        Error(nameof(UserAlreadyInRole), $"Користувач уже має роль «{role}».");

    public override IdentityError UserNotInRole(string role) =>
        Error(nameof(UserNotInRole), $"Користувач не має ролі «{role}».");

    public override IdentityError ConcurrencyFailure() =>
        Error(nameof(ConcurrencyFailure), "Дані змінилися в іншому місці. Оновіть сторінку й спробуйте ще раз.");

    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };
}
