using Huybrechts.App.Config;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Huybrechts.App.Services;

public class AzurePricingService
{
    private readonly PlatformImportOptions _options;

    private static async Task<PricingResponse?> GetPricingItemsAsync(string requestUrl, bool region)
    {
        PricingResponse? pricingResult = null;
        var uniqueset = new HashSet<string>();
        int currentPage = 0;
        int maxPages = 20;

        using HttpClient httpClient = new();

        while (!string.IsNullOrEmpty(requestUrl) && currentPage < maxPages)
        {
            try
            {
                var response = await httpClient.GetStringAsync(requestUrl);
                var pricingResponse = JsonConvert.DeserializeObject<PricingResponse>(response);
                if (pricingResponse is not null)
                {
                    pricingResult ??= new()
                        {
                            BillingCurrency = pricingResponse.BillingCurrency,
                            CustomerEntityId = pricingResponse.CustomerEntityId,
                            CustomerEntityType = pricingResponse.CustomerEntityType,
                            Items = []
                        };

                    if (pricingResponse.Items is not null && pricingResponse.Items.Count > 0)
                    {
                        if (region)
                        {
                            foreach (var item in pricingResponse.Items ?? [])
                            {
                                if (!string.IsNullOrEmpty(item.ArmRegionName) && uniqueset.Add(item.ArmRegionName))
                                {
                                    pricingResult.Items!.Add(item);
                                    pricingResult.Count += 1;
                                }
                            }
                        }
                        else
                        { 
                            pricingResult.Items!.AddRange(pricingResponse.Items);
                            pricingResult.Count += pricingResponse.Count;
                        }
                    }

                    requestUrl = pricingResponse.NextPageLink ?? string.Empty;
                    currentPage++;
                }
            }
            catch (HttpRequestException)
            {
                break;
            }
        }

        return pricingResult;
    }

    public class PricingResponse
    {
        public string? BillingCurrency { get; set; }

        public string? CustomerEntityId { get; set; }

        public string? CustomerEntityType { get; set; }

        public List<PricingItem>? Items { get; set; }

        public string? NextPageLink { get; set; }

        public int Count { get; set; }
    }

    public class PricingItem
    {
        /// <summary>
        /// "currencyCode":"USD"
        /// </summary>
        [JsonPropertyName("currencyCode")]
        public string? CurrencyCode { get; set; }

        /// <summary>
        /// "tierMinimumUnits":0.0
        /// </summary>
        [JsonPropertyName("tierMinimumUnits")]
        public double TierMinimumUnits { get; set; }

        /// <summary>
        /// "retailPrice":0.29601
        /// </summary>
        [JsonPropertyName("retailPrice")]
        public double RetailPrice { get; set; }

        /// <summary>
        /// "unitPrice":0.29601
        /// </summary>
        [JsonPropertyName("unitPrice")]
        public double UnitPrice { get; set; }

        /// <summary>
        /// "armRegionName":"southindia"
        /// </summary>
        [JsonPropertyName("armRegionName")]
        public string? ArmRegionName { get; set; }

        /// <summary>
        /// "location":"IN South"
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// "":"2024-08-01T00:00:00Z"
        /// </summary>
        [JsonPropertyName("effectiveStartDate")]
        public string? EffectiveStartDate { get; set; }

        /// <summary>
        /// "meterId":"000009d0-057f-5f2b-b7e9-9e26add324a8"
        /// </summary>
        [JsonPropertyName("meterId")]
        public string? MeterId { get; set; }

        /// <summary>
        /// "meterName":"D14/DS14 Spot"
        /// </summary>
        [JsonPropertyName("meterName")]
        public string? MeterName { get; set; }

        /// <summary>
        /// "productId":"DZH318Z0BPVW"
        /// </summary>
        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }

        /// <summary>
        /// "skuId":"DZH318Z0BPVW/00QZ"
        /// </summary>
        [JsonPropertyName("skuId")]
        public string? SkuId { get; set; }

        /// <summary>
        /// "productName":"Virtual Machines D Series Windows"
        /// </summary>
        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }

        /// <summary>
        /// "skuName":"D14 Spot"
        /// </summary>
        [JsonPropertyName("skuName")]
        public string? SkuName { get; set; }

        /// <summary>
        /// "serviceName":"Virtual Machines"
        /// </summary>
        [JsonPropertyName("serviceName")]
        public string? ServiceName { get; set; }

        /// <summary>
        /// "serviceId":"DZH313Z7MMC8"
        /// </summary>
        [JsonPropertyName("serviceId")]
        public string? ServiceId { get; set; }

        /// <summary>
        /// "serviceFamily":"Compute"
        /// </summary>
        [JsonPropertyName("serviceFamily")]
        public string? ServiceFamily { get; set; }

        /// <summary>
        /// "unitOfMeasure":"1 Hour"
        /// </summary>
        [JsonPropertyName("unitOfMeasure")]
        public string? UnitOfMeasure { get; set; }

        /// <summary>
        /// "type":"Consumption"
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// "isPrimaryMeterRegion": true
        /// </summary>
        [JsonPropertyName("isPrimaryMeterRegion")]
        public bool IsPrimaryMeterRegion { get; set; }

        /// <summary>
        /// "armSkuName":"Standard_D14"
        /// </summary>
        [JsonPropertyName("armSkuName")]
        public string? ArmSkuName { get; set; }
    }

    public AzurePricingService(PlatformImportOptions options)
    {
        _options = options;
    }

    public async Task<PricingResponse?> GetRegionsAsync()
    {
        return await GetPricingItemsAsync(_options.Platforms["Azure"].Regions, true);
    }
}
