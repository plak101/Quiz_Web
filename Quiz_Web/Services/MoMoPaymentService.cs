using Microsoft.Extensions.Options;
using Quiz_Web.Models.MoMoPayment;
using Quiz_Web.Services.IServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace Quiz_Web.Services
{
    public class MoMoPaymentService : IMoMoPaymentService
    {
        private readonly MoMoSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoMoPaymentService> _logger;

        public MoMoPaymentService(
            IOptions<MoMoSettings> settings,
            HttpClient httpClient,
            ILogger<MoMoPaymentService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MoMoPaymentResponse> CreatePaymentAsync(decimal amount, string orderInfo, string orderId)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var extraData = string.Empty;

                // Amount phải là số nguyên (VND)
                var amountLong = (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
                var amountStr = amountLong.ToString(CultureInfo.InvariantCulture);

                // Thứ tự tham số theo alphabet (a-z)
                var rawData = $"accessKey={_settings.AccessKey}" +
                             $"&amount={amountStr}" +
                             $"&extraData={extraData}" +
                             $"&ipnUrl={_settings.IpnUrl}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo={orderInfo}" +
                             $"&partnerCode={_settings.PartnerCode}" +
                             $"&redirectUrl={_settings.RedirectUrl}" +
                             $"&requestId={requestId}" +
                             $"&requestType=captureWallet";

                var signature = GenerateSignature(rawData);

                _logger.LogInformation($"Raw data for signature: {rawData}");
                _logger.LogInformation($"Generated signature: {signature}");

                var request = new MoMoPaymentRequest
                {
                    partnerCode = _settings.PartnerCode,
                    accessKey = _settings.AccessKey, // include in body
                    requestId = requestId,
                    amount = amountLong,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = _settings.RedirectUrl,
                    ipnUrl = _settings.IpnUrl,
                    extraData = extraData,
                    requestType = "captureWallet",
                    signature = signature,
                    lang = "vi"
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonRequest = JsonSerializer.Serialize(request, jsonOptions);
                _logger.LogInformation($"MoMo Request: {jsonRequest}");

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_settings.ApiEndpoint, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"MoMo Response: {jsonResponse}");

                var momoResponse = JsonSerializer.Deserialize<MoMoPaymentResponse>(jsonResponse);
                return momoResponse ?? new MoMoPaymentResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                throw;
            }
        }

        public bool ValidateSignature(MoMoIpnRequest ipnRequest)
        {
            try
            {
                var amountStr = ipnRequest.amount.ToString(CultureInfo.InvariantCulture);
                // Thứ tự tham số theo alphabet (a-z)
                var rawData = $"accessKey={_settings.AccessKey}" +
                             $"&amount={amountStr}" +
                             $"&extraData={ipnRequest.extraData}" +
                             $"&message={ipnRequest.message}" +
                             $"&orderId={ipnRequest.orderId}" +
                             $"&orderInfo={ipnRequest.orderInfo}" +
                             $"&orderType={ipnRequest.orderType}" +
                             $"&partnerCode={ipnRequest.partnerCode}" +
                             $"&payType={ipnRequest.payType}" +
                             $"&requestId={ipnRequest.requestId}" +
                             $"&responseTime={ipnRequest.responseTime}" +
                             $"&resultCode={ipnRequest.resultCode}" +
                             $"&transId={ipnRequest.transId}";

                var signature = GenerateSignature(rawData);
                return signature.Equals(ipnRequest.signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MoMo signature");
                return false;
            }
        }

        public string GenerateSignature(string rawData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_settings.SecretKey);
            var messageBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
