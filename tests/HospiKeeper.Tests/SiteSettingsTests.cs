using HospiKeeper.Contracts;
using HospiKeeper.Domain.Common;
using Xunit;

namespace HospiKeeper.Tests;

/// <summary>系统配置：默认值、管理员保存与前台越权校验。</summary>
public sealed class SiteSettingsTests
{
    [Fact]
    public async Task PublicSettings_ReturnsDefaults_WhenNotConfigured()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var settings = await ctx.SiteSettings.GetPublicAsync();

        Assert.Equal("云栖民宿", settings.BrandName);
        Assert.Equal("山与湖", settings.HeroAccent);
        Assert.NotEmpty(settings.Features);
        Assert.NotEmpty(settings.Notices);
    }

    [Fact]
    public async Task Admin_Update_PersistsAndReadsBack()
    {
        await using var ctx = await ServiceTestContext.CreateAsync();
        var settings = await ctx.SiteSettings.GetForAdminAsync();
        settings.BrandName = "测试民宿";
        settings.HeroAccent = "测试湖";
        settings.Rating = 4.8;

        var saved = await ctx.SiteSettings.UpdateAsync(settings);

        Assert.Equal("测试民宿", saved.BrandName);
        Assert.Equal("老陈（管理员）", saved.UpdatedBy);
        Assert.NotNull(saved.UpdatedAt);

        var publicSettings = await ctx.SiteSettings.GetPublicAsync();
        Assert.Equal("测试民宿", publicSettings.BrandName);
        Assert.Equal("测试湖", publicSettings.HeroAccent);
        Assert.Equal(4.8, publicSettings.Rating);
    }

    [Fact]
    public async Task FrontDesk_CannotUpdateSettings()
    {
        await using var ctx = await ServiceTestContext.CreateAsync("front01");
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            ctx.SiteSettings.UpdateAsync(new SiteSettings()));
    }
}
