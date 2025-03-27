using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;

public class IndexModel : PageModel
{
    public List<Company> Companies { get; set; } = new();
    public string? SelectedSymbol { get; set; }
    public string? StockData { get; set; }
    public List<StockPrice>? ProcessedStockPrices { get; set; }
    public string? PriceTrend { get; set; }
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;

        _connectionString =
            configuration.GetConnectionString("PostgresDB") ??
            Environment.GetEnvironmentVariable("PostgresDB") ??
            throw new InvalidOperationException("Connection string 'PostgresDB' not found.");
    }

    public void OnGet()
    {
        string filePath = "wwwroot/companySymbols.csv";
        if (System.IO.File.Exists(filePath))
        {
            Companies = ReadCsvFile(filePath);
        }
    }

    public async Task OnPostAsync(string symbol)
    {
        SelectedSymbol = symbol;
        using var httpClient = new HttpClient();
        var apiKey = "O9YJ669QXO4MCGS8";
        var requestUrl = $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval=5min&apikey={apiKey}";

        try
        {
            StockData = await httpClient.GetStringAsync(requestUrl);

            if (string.IsNullOrEmpty(StockData) || StockData.Contains("Thank you for using Alpha Vantage!"))
            {
                StockData = "API rate limit exceeded or invalid response.";
                return;
            }

            ProcessStockData();
            if (ProcessedStockPrices != null && ProcessedStockPrices.Any())
            {
                SaveProcessedStockData(ProcessedStockPrices);
            }
            OnGet();
        }
        catch (HttpRequestException ex)
        {
            StockData = "Error fetching stock data: " + ex.Message;
        }
    }

    private void ProcessStockData()
    {
        if (string.IsNullOrEmpty(StockData)) return;

        var jsonDoc = JsonDocument.Parse(StockData);
        if (!jsonDoc.RootElement.TryGetProperty("Time Series (5min)", out var timeSeries)) return;

        ProcessedStockPrices = timeSeries.EnumerateObject()
            .Select(entry => new StockPrice
            {
                Time = DateTime.ParseExact(entry.Name, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                ClosePrice = decimal.Parse(entry.Value.GetProperty("4. close").GetString()!, CultureInfo.InvariantCulture),
                Symbol = SelectedSymbol ?? string.Empty,
                MovingAverage = 0,
                Trend = string.Empty
            })
            .OrderBy(sp => sp.Time)
            .ToList();

        if (ProcessedStockPrices.Count >= 5)
        {
            decimal movingAverage = ProcessedStockPrices.TakeLast(5).Average(sp => sp.ClosePrice);
            string trend = ProcessedStockPrices[^1].ClosePrice > movingAverage ? "Uptrend" : "Downtrend";

            foreach (var stock in ProcessedStockPrices)
            {
                stock.MovingAverage = movingAverage;
                stock.Trend = trend;
            }
            PriceTrend = trend;
        }
    }

    public void SaveProcessedStockData(List<StockPrice> processedStocks)
    {
        if (processedStocks == null || !processedStocks.Any())
            return;

        Console.WriteLine($"Using connection string: {_connectionString}");

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        foreach (var stock in processedStocks)
        {
            using var command = new NpgsqlCommand(
            "INSERT INTO processed_stock_prices (symbol, timestamp, close_price, moving_average, trend) " +
            "VALUES (@symbol, @timestamp, @close_price, @moving_average, @trend) " +
            "ON CONFLICT (symbol, timestamp) DO NOTHING", connection);

            command.Parameters.AddWithValue("symbol", stock.Symbol ?? string.Empty);
            command.Parameters.AddWithValue("timestamp", stock.Time);
            command.Parameters.AddWithValue("close_price", stock.ClosePrice);
            command.Parameters.AddWithValue("moving_average", stock.MovingAverage);
            command.Parameters.AddWithValue("trend", stock.Trend ?? string.Empty);

            command.ExecuteNonQuery();
        }
    }

    private List<Company> ReadCsvFile(string filePath)
    {
        var lines = System.IO.File.ReadAllLines(filePath).Skip(1);
        return lines
            .Select(line => line.Split(','))
            .Where(values => values.Length >= 2)
            .Select(values => new Company { Symbol = values[0].Trim(), Name = values[1].Trim() })
            .ToList();
    }
}

public class Company
{
    public string? Symbol { get; set; }
    public string? Name { get; set; }
}

public class StockPrice
{
    public DateTime Time { get; set; }
    public decimal ClosePrice { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal MovingAverage { get; set; }
    public string Trend { get; set; } = string.Empty;
}