using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace DinkumCoin.Api.Client
{
    public class DinkumCoinApiClient
    {

        public string BaseUri { get; set; }
        private const string GET_WALLETS_RESOURCE = "/api/wallets";
        private const string GET_WALLET_BY_ID_RESOURCE = "/api/wallets/{0}";
        private const string MINE_COIN_RESOURCE = "/api/wallets/{0}/minecoin";


        public DinkumCoinApiClient(string _baseUri)
        {
            BaseUri = _baseUri;
        }



        public Dictionary<string, string> GetAllWallets()
        {
            HttpStatusCode statusCode;

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, BaseUri.TrimEnd('/') + GET_WALLETS_RESOURCE);


                var response = client.SendAsync(request);
                var content = response.Result.Content.ReadAsStringAsync().Result;
                statusCode = response.Result.StatusCode;

                request.Dispose();
                response.Result.Dispose();

                if (statusCode == HttpStatusCode.OK)
                {
                    return !String.IsNullOrEmpty(content) ?
                      JsonConvert.DeserializeObject<Dictionary<string, string>>(content)
                      : null;
                }
                throw new Exception($"Unexpected HTTP Status code in response, received {statusCode}: {content}");
            }
        }
    }
}