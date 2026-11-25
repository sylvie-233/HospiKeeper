using System.Text.Json;
using System.Text.Json.Serialization;
using HospiKeeper.Contracts;
using HospiKeeper.Domain.Enums;
using Xunit;

namespace HospiKeeper.Tests;

/// <summary>验证 API 契约 DTO 能被客户端按相同 JSON 形状反序列化。</summary>
public sealed class ContractSerializationTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private const string CheckInJson = """
        {
          "id": 1,
          "checkInNo": "I20261125001",
          "roomId": 1,
          "roomNo": "101",
          "roomTypeName": "大床房",
          "guestName": "张三",
          "guestPhone": "13800138000",
          "guestIdCard": "110101199001011234",
          "checkInTime": "2026-11-25T10:00:00",
          "planOutTime": "2026-11-26T12:00:00",
          "actualOutTime": null,
          "price": 200,
          "originPrice": null,
          "deposit": 100,
          "extendHours": 0,
          "status": "Living",
          "note": null,
          "opUserId": 1,
          "operatorName": "老陈（管理员）",
          "createdAt": "2026-11-25T10:00:00",
          "companions": [
            { "id": 1, "checkInId": 1, "name": "李四", "phone": null, "idCard": null }
          ],
          "extraCharges": [
            {
              "id": 1,
              "checkInId": 1,
              "item": "矿泉水",
              "amount": 3,
              "chargeTime": "2026-11-25T11:00:00",
              "opUserId": 1,
              "operatorName": "老陈（管理员）"
            }
          ],
          "extraTotal": 3
        }
        """;

    [Fact]
    public void CheckInRecordDto_DeserializesNestedCollections()
    {
        var dto = JsonSerializer.Deserialize<CheckInRecordDto>(CheckInJson, Options);

        Assert.NotNull(dto);
        Assert.Equal(CheckInStatus.Living, dto!.Status);
        Assert.Equal(3m, dto.ExtraTotal);
        Assert.Single(dto.Companions);
        Assert.Equal("李四", dto.Companions[0].Name);
        Assert.Single(dto.ExtraCharges);
        Assert.Equal("矿泉水", dto.ExtraCharges[0].Item);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
