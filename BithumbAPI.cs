using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using QuickShot;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Markup;

namespace BithumbTrader
{
    public class BithumbAPI
    {
        #region 인증 및 요청 영역

        /// <summary>
        /// 빗썸 api를 직접적으로 요청하기 위해 사용할 수 있습니다.
        /// </summary>
        ///<param name="method">Method.Get Post Delete 등</param>
        ///<param name="subUrl">빗썸 api 서브 url</param>
        ///<returns>RestResponse</returns>
        public async Task<RestResponse> GetResponse(Method method, string subUrl)
        {
            await CheckHeartBeat();
            var restClient = new RestClient("https://api.bithumb.com");
            var request = new RestRequest(subUrl, method);//.AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Token)
            return await restClient.ExecuteAsync(request);
        }

        /// <summary>
        /// 빗썸 api를 직접적으로 요청하기 위해 사용할 수 있습니다. 추가적으로 json object를 데이터에 추가합니다.
        /// private API를 사용하기 위해 Bearer Token을 자동적으로 헤더에 추가됩니다.
        /// </summary>
        ///<param name="method">Method.Get Post Delete 등</param>
        ///<param name="subUrl">빗썸 api 서브 url</param>
        ///<param name="json">요청할 데이터</param>
        ///<returns>RestResponse</returns>
        public async Task<RestResponse> GetResponse(Method method, string subUrl, JObject json)
        {
            await CheckHeartBeat();
            var restClient = new RestClient("https://api.bithumb.com");
            var request = new RestRequest(subUrl, method)
                .AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Token)
                .AddHeader("accept", "application/json")
                .AddJsonBody(json.ToString(), false);
            return await restClient.ExecuteAsync(request);
        }

        /// <summary>
        /// 빗썸 api를 직접적으로 요청하기 위해 사용할 수 있습니다.
        /// 추가적으로 Bearer Token을 헤더에 추가합니다.
        /// </summary>
        ///<param name="method">Method.Get Post Delete 등</param>
        ///<param name="subUrl">빗썸 api 서브 url</param>
        ///<param name="token">헤더에 사용될 Bearer Token</param>
        ///<returns>RestResponse</returns>
        public async Task<RestResponse> GetResponse(Method method, string subUrl, string token)
        {
            await CheckHeartBeat();
            var restClient = new RestClient("https://api.bithumb.com");
            var request =
                new RestRequest(subUrl, method).AddHeader("Authorization", "Bearer " + token);
            return await restClient.ExecuteAsync(request);
        }

        /// <summary>
        /// 빗썸에서 발급받은 ApiKey와 SecretKey를 사용하여 JWT 토큰을 발급합니다.
        /// </summary>
        ///<param name="apiKey">빗썸에서 발급받은 ApiKey</param>
        ///<param name="secretKey">빗썸에서 발급받은 SecretKey</param>
        ///<returns>JWT 토큰을 string 형태로 반환</returns>
        public string IssueJwtToken(string apiKey, string secretKey)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("access_key", apiKey),
                new Claim("nonce", Guid.NewGuid().ToString()),
                new Claim("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion

        #region Public API 영역
        /// <summary>
        /// 경보중인 마켓-코인 목록 조회
        /// </summary>
        ///<returns>JObject</returns>
        public async Task<JToken> GetVirtualAssetWarning()
        {
            var response = await GetResponse(Method.Get, "/v1/market/virtual_asset_warning");
            return ParseResponse(response);
        }

        /// <summary>
        /// <para>현재가 정보 : 요청 시점 종목의 스냅샷이 제공됩니다.</para>
        /// 반점으로 구분되는 마켓 코드 (ex. KRW-BTC, BTC-ETH)
        /// </summary>
        ///<returns>JObject</returns>
        public async Task<JToken> GetTicker(string markets)
        {
            var response = await GetResponse(Method.Get, "/v1/ticker?markets=" + HttpUtility.UrlEncode(markets));
            return ParseResponse(response);
        }

        /// <summary>
        /// <para>빗썸에서 거래 가능한 마켓과 가상자산 정보를 제공합니다.</para>
        /// </summary>
        ///<returns>JObject</returns>
        public async Task<JToken> GetMarketAll(string markets, bool isDetails)
        {
            var response = await GetResponse(Method.Get, "/v1/market/all" + (isDetails ? "?isDetails=true" : ""));
            return ParseResponse(response);
        }
        #endregion

        #region Private API 영역

        /// <summary>
        /// 보유 중인 자산 정보를 조회합니다.
        /// </summary>
        ///<returns>JObject</returns>
        public async Task<JToken> GetAccounts()
        {
            var response = await GetResponse(Method.Get, "/v1/accounts");
            return ParseResponse(response);
        }

        /// <summary>
        /// 지정가 매수 또는 매도 주문을 요청합니다. 
        /// </summary>
        ///<returns>JObject</returns>
        public async Task<JToken> RequestOrder(OrderType type, string market, double volume, double price)
        {
            JObject data = new JObject
                {
                    { "market", market },
                    { "side", ParseEnum(type) },
                    { "volume", volume.ToString(CultureInfo.InvariantCulture) },
                    { "price", price.ToString(CultureInfo.InvariantCulture) },
                    { "ord_type", "limit" }
                };
            Console.WriteLine("Request Order: " + data.ToString());
            var response = await GetResponse(Method.Get, "/v1/orders", data);
            return ParseResponse(response);
        }

        /// <summary>
        /// 지정가 주문을 취소 요청합니다.
        /// </summary>
        ///<returns>JObject</returns>
        public async Task<JToken> RequestOrder(OrderType type, string uuid)
        {
            var response = await GetResponse(Method.Delete, "/v1/orders?uuid=" + uuid, Properties.Settings.Default.Token);
            return ParseResponse(response);
        }


        #endregion

        #region 내부 함수 영역

        public enum OrderType
        {
            Buy = 0,
            Sell =1,
            Cancel=2
        }

        private string ParseEnum(OrderType ot)
        {
            //bid 매수 , ask 매도
            return ot == OrderType.Buy ? "bid" : "ask";
        }

        private static JObject GetErrorTemplate()
        {
            var json = new JObject();
            var json2 = new JObject
            {
                { "name", "program_internal_error" },
                { "message", "프로그램 내부적으로 오류가 발생하였습니다." }
            };
            json.Add("error", json2);

            return json;
        }

        private async Task CheckHeartBeat()
        {
            var response = await GetResponse(Method.Get, "/v1/accounts");
            //expired_jwt 토큰 만료시 확인 코드 및 재발급 수행 
            if (response.Content == null) return;
            JObject json = JObject.Parse(response.Content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("JWT Heartbeat complete. | " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            }
            else
            {
                string code = json["error"]["name"].ToString();
                if (code == "expired_jwt" || code == "jwt_verification")
                {
                    Console.WriteLine("JWT REFRESHED!!! | " + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    Properties.Settings.Default.Token = IssueJwtToken(
                        new Enc().decrypt(Properties.Settings.Default.APIKEY),
                        new Enc().decrypt(Properties.Settings.Default.SECRETKEY));
                    Properties.Settings.Default.Save();
                }
            }
        }

        private JToken ParseResponse(RestResponse response)
        {
            if (response?.Content == null) return GetErrorTemplate();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JToken jToken = JArray.Parse(response.Content);
                return jToken;
            }
            else
            {
                return JObject.Parse(response.Content);
            }
        }
        #endregion
    }
}
