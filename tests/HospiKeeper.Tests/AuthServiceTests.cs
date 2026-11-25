using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using HospiKeeper.Domain.Enums;
using Xunit;

namespace HospiKeeper.Tests;

/// <summary>登录、停用与权限校验的回归测试。</summary>
public sealed class AuthServiceTests
{
    [Fact]
    public async Task Login_Success_ReturnsAdmin()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        Assert.True(ctx.Session.IsAdmin);
        Assert.Equal("老陈（管理员）", ctx.Session.DisplayName);
    }

    [Fact]
    public async Task Login_WrongPassword_Fails()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var result = await ctx.Auth.LoginAsync("admin", "bad-password");
        Assert.False(result.Success);
        Assert.Contains("密码错误", result.Error);
    }

    [Fact]
    public async Task Login_DisabledAccount_Fails()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var result = await ctx.Auth.LoginAsync("front02", "123456");
        Assert.False(result.Success);
        Assert.Contains("停用", result.Error);
    }

    [Fact]
    public async Task Login_UnknownAccount_Fails()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var result = await ctx.Auth.LoginAsync("nobody", "123456");
        Assert.False(result.Success);
        Assert.Contains("不存在", result.Error);
    }

    [Fact]
    public async Task CannotDisableCurrentAccount()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var current = ctx.Session.CurrentUser!;
        await Assert.ThrowsAsync<ForbiddenException>(
            () => ctx.Auth.SetUserStatusAsync(current.Id, AccountStatus.Disabled));
    }

    [Fact]
    public async Task FrontDesk_CannotListAccounts()
    {
        await using var ctx = await ServiceTestContext.CreateAsync("front01");
        await Assert.ThrowsAsync<ForbiddenException>(() => ctx.Auth.GetUsersAsync());
    }

    [Fact]
    public async Task CreateUser_RejectsDuplicateUsername()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        await Assert.ThrowsAsync<DomainException>(() => ctx.Auth.CreateUserAsync(new AccountInput(
            "front01", "重复账号", UserRole.Front, AccountStatus.Active, "123456")));
    }

    [Fact]
    public async Task ChangeOwnPassword_ThenLoginWithNewPassword()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        await ctx.Auth.ChangeOwnPasswordAsync("123456", "newpass123", "newpass123");
        await ctx.Auth.LogoutAsync();

        var result = await ctx.Auth.LoginAsync("admin", "newpass123");
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ChangeOwnPassword_RejectsMismatchedConfirmation()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        await Assert.ThrowsAsync<DomainException>(() =>
            ctx.Auth.ChangeOwnPasswordAsync("123456", "newpass123", "different"));
    }
}
