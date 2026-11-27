using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ExpenseTracker.Tests;

public sealed class ExpenseCurrencyApiTests(PostgresApiFactory factory)
{
    private const string Endpoint = "/api/expenses";

    [Fact]
    public async Task Should_default_to_HUF_when_currency_is_omitted()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();
        var payload = new { amount = 1200m, description = "Lunch", date = "2026-04-01" };

        var response = await client.PostAsJsonAsync(Endpoint, payload, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.Equal("HUF", created.GetProperty("currency").GetString());

        var fetched = await client.GetFromJsonAsync<JsonElement>(response.Headers.Location, ct);
        Assert.Equal("HUF", fetched.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task Should_default_to_HUF_when_currency_is_null()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, Payload(currency: null), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.Equal("HUF", created.GetProperty("currency").GetString());
    }

    [Theory]
    [InlineData("HUF")]
    [InlineData("EUR")]
    [InlineData("USD")]
    public async Task Should_return_201_and_persist_currency_when_currency_is_supported(string currency)
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, Payload(currency), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.Equal(currency, created.GetProperty("currency").GetString());

        var id = created.GetProperty("id").GetString();
        var listed = await client.GetFromJsonAsync<JsonElement>(Endpoint, ct);
        var persisted = listed.EnumerateArray().Single(e => e.GetProperty("id").GetString() == id);
        Assert.Equal(currency, persisted.GetProperty("currency").GetString());
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("eur")]
    [InlineData(" EUR")]
    [InlineData("EUR ")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("EURO")]
    [InlineData("HUF,EUR")]
    public async Task Should_return_400_problem_details_when_currency_is_invalid(string currency)
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, Payload(currency), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        Assert.True(problem.GetProperty("errors").TryGetProperty("Currency", out _));
    }

    [Fact]
    public async Task Should_report_all_errors_when_currency_and_amount_are_invalid()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Endpoint, Payload("GBP", amount: 0m), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<JsonElement>(ct)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("Amount", out _));
        Assert.True(errors.TryGetProperty("Currency", out _));
    }

    private static object Payload(string? currency, decimal amount = 10m) =>
        new { amount, description = "Currency test", date = "2026-04-01", currency };
}
