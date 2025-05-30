﻿using Huybrechts.App.Config;
using Huybrechts.Core.Setup;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Huybrechts.App.Services;

public class AzurePricingService
{
    private enum ServiceType
    {
        All = 0,
        Regions = 1,
        Services = 2,
        Products = 3,
        Rates = 4,
        Units = 5
    }

    private readonly PlatformImportOptions _options;
    private static readonly object _cacheLock = new();
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _expiration;

    private static async Task<PricingResponse?> GetPricingItemsAsync(
        PlatformImportOptionSettings request, 
        ServiceType type,
        string currency, 
        string service, 
        string location,
        string searchString)
    {
        PricingResponse? pricingResult = null;
        var uniqueset = new HashSet<string>();
        string requestUrl = string.Empty;
        int currentPage = 0;
        int maxPages = 50;

        using HttpClient httpClient = new();

        if (ServiceType.Regions == type)
        {
            requestUrl = request.RegionUrl;
            if (!string.IsNullOrEmpty(searchString))
                requestUrl += request.RegionSearch.Replace("{0}", searchString);
        }
        else if (ServiceType.Services == type)
        {
            requestUrl = request.ServiceUrl;
            if (!string.IsNullOrEmpty(searchString))
                requestUrl += request.ServiceSearch.Replace("{0}", searchString);
        }
        else if (ServiceType.Products == type)
        {
            requestUrl = request.ProductUrl;
            if (!string.IsNullOrEmpty(searchString))
                requestUrl += request.ProductSearch.Replace("{0}", searchString);
        }
        else if (ServiceType.Units == type)
        {
            requestUrl = request.UnitUrl;
            if (!string.IsNullOrEmpty(searchString))
                requestUrl += request.UnitSearch.Replace("{0}", searchString);
        }
        else
        {
            requestUrl = request.RatesUrl.Replace("{currency}", currency).Replace("{service}", service).Replace("{location}", location);
            if (!string.IsNullOrEmpty(searchString))
                requestUrl += request.RatesSearch.Replace("{0}", searchString);
        }

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
                        if (ServiceType.Regions == type)
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
                        else if (ServiceType.Services == type)
                        {
                            foreach (var item in pricingResponse.Items ?? [])
                            {
                                if (!string.IsNullOrEmpty(item.ServiceName) && uniqueset.Add(item.ServiceName))
                                {
                                    pricingResult.Items!.Add(item);
                                    pricingResult.Count += 1;
                                }
                            }
                        }
                        else if (ServiceType.Products == type)
                        {
                            foreach (var item in pricingResponse.Items ?? [])
                            {
                                if (!string.IsNullOrEmpty(item.ProductName) && uniqueset.Add(item.ProductName))
                                {
                                    pricingResult.Items!.Add(item);
                                    pricingResult.Count += 1;
                                }
                            }
                        }
                        else if (ServiceType.Units == type)
                        {
                            foreach (var item in pricingResponse.Items ?? [])
                            {
                                if (!string.IsNullOrEmpty(item.UnitOfMeasure) && uniqueset.Add(item.UnitOfMeasure))
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

    public AzurePricingService(PlatformImportOptions options, IMemoryCache cache)
    {
        _expiration = ApplicationSettings.GetCacheExpirationTime();
        _options = options;
        _cache = cache;
    }

    public async Task<PricingResponse?> GetRegionsAsync(string currency, string service, string location, string searchString)
    {
        var cacheKey = $"PlatformRegionListForAzure_{searchString}";

        if (_cache.TryGetValue(cacheKey, out PricingResponse? cachedResponse))
        {
            if (cachedResponse is not null)
                return cachedResponse;
        }

        PricingResponse? response = await GetPricingItemsAsync(
            _options.Platforms["Azure"],
            ServiceType.Regions,
            currency,
            service,
            location,
            searchString);

        if (response is not null)
        {
            _cache.Set(cacheKey, response, _expiration);
        }

        return response;
    }

    public async Task<PricingResponse?> GetServicesAsync(string currency, string service, string location, string searchString)
    {
        var cacheKey = $"PlatformServiceListForAzure_{searchString}";

        if (_cache.TryGetValue(cacheKey, out PricingResponse? cachedResponse))
        {
            if (cachedResponse is not null)
                return cachedResponse;
        }

        PricingResponse? response = await GetPricingItemsAsync(
            _options.Platforms["Azure"],
            ServiceType.Services,
            currency,
            service,
            location,
            searchString);

        if (response is not null)
        {
            _cache.Set(cacheKey, response, _expiration);
        }

        return response;
    }

    public async Task<PricingResponse?> GetProductsAsync(string currency, string service, string location, string searchString)
    {
        var cacheKey = $"PlatformProductListForAzure_{searchString}";

        if (_cache.TryGetValue(cacheKey, out PricingResponse? cachedResponse))
        {
            if (cachedResponse is not null)
                return cachedResponse;
        }

        PricingResponse? response = await GetPricingItemsAsync(
            _options.Platforms["Azure"],
            ServiceType.Products,
            currency,
            service,
            location,
            searchString);

        if (response is not null)
        {
            _cache.Set(cacheKey, response, _expiration);
        }

        return response;
    }

    public async Task<PricingResponse?> GetRatesAsync(string currency, string service, string location, string searchString)
    {
        var cacheKey = $"PlatformRateListForAzure_{currency}_{service}_{location}_{searchString}";

        if (_cache.TryGetValue(cacheKey, out PricingResponse? cachedResponse))
        {
            if (cachedResponse is not null)
                return cachedResponse;
        }

        PricingResponse? response = await GetPricingItemsAsync(
            _options.Platforms["Azure"],
            ServiceType.Rates,
            currency,
            service,
            location,
            searchString);

        if (response is not null)
        {
            _cache.Set(cacheKey, response, _expiration);
        }

        return response;
    }

    public async Task<PricingResponse?> GetUnitsAsync(string searchString)
    {
        var cacheKey = $"PlatformUnitListForAzure_{searchString}";

        if (_cache.TryGetValue(cacheKey, out PricingResponse? cachedResponse))
        {
            if (cachedResponse is not null)
                return cachedResponse;
        }

        PricingResponse? response = await GetPricingItemsAsync(
            _options.Platforms["Azure"],
            ServiceType.Units,
            "",
            "",
            "",
            searchString);

        if (response is not null)
        {
            _cache.Set(cacheKey, response, _expiration);
        }

        return response;
    }
}
