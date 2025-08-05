# Bithumb API Wrapper

C#으로 구현된 빗썸 API Wrapper입니다.  
공개(Public) 및 비공개(Private) API 호출을 지원합니다.
현재는 개발자 본인이 필요한 기능만 추가한 상태이며, 추후 다른 API도 추가될 수 있습니다. 

RESTSHARP 를 사용합니다.
---

## 🔐 인증 및 요청 영역

### `GetResponse(Method method, string subUrl)`
> 빗썸 api를 직접적으로 요청하기 위해 사용할 수 있습니다.

```csharp
public async Task<RestResponse> GetResponse(Method method, string subUrl)
```

---

### `GetResponse(Method method, string subUrl, JObject json)`
> 빗썸 api를 직접적으로 요청하기 위해 사용할 수 있습니다. 추가적으로 json object를 데이터에 추가합니다.  
> private API를 사용하기 위해 Bearer Token을 자동적으로 헤더에 추가됩니다.

```csharp
public async Task<RestResponse> GetResponse(Method method, string subUrl, JObject json)
```

---

### `GetResponse(Method method, string subUrl, string token)`
> 빗썸 api를 직접적으로 요청하기 위해 사용할 수 있습니다.  
> 추가적으로 Bearer Token을 헤더에 추가합니다.

```csharp
public async Task<RestResponse> GetResponse(Method method, string subUrl, string token)
```

---

### `IssueJwtToken(string apiKey, string secretKey)`
> 빗썸에서 발급받은 ApiKey와 SecretKey를 사용하여 JWT 토큰을 발급합니다.

```csharp
public string IssueJwtToken(string apiKey, string secretKey)
```

---

## 🌐 Public API 영역

### `GetVirtualAssetWarning()`
> 경보중인 마켓-코인 목록 조회

```csharp
public async Task<JToken> GetVirtualAssetWarning()
```

---

### `GetTicker(string markets)`
> 현재가 정보 : 요청 시점 종목의 스냅샷이 제공됩니다.  
> 반점으로 구분되는 마켓 코드 (ex. KRW-BTC, BTC-ETH)

```csharp
public async Task<JToken> GetTicker(string markets)
```

---

### `GetMarketAll(string markets, bool isDetails)`
> 빗썸에서 거래 가능한 마켓과 가상자산 정보를 제공합니다.

```csharp
public async Task<JToken> GetMarketAll(string markets, bool isDetails)
```

---

## 🔒 Private API 영역

### `GetAccounts()`
> 보유 중인 자산 정보를 조회합니다.

```csharp
public async Task<JToken> GetAccounts()
```

---

### `RequestOrder(OrderType type, string market, double volume, double price)`
> 지정가 매수 또는 매도 주문을 요청합니다.

```csharp
public async Task<JToken> RequestOrder(OrderType type, string market, double volume, double price)
```

---

### `RequestOrder(OrderType type, string uuid)`
> 지정가 주문을 취소 요청합니다.

```csharp
public async Task<JToken> RequestOrder(OrderType type, string uuid)
```
---

## ⚙ Enum

```csharp
public enum OrderType
{
    Buy = 0,
    Sell = 1,
    Cancel = 2
}
```

---

## 📄 라이선스

> MIT LICENSE
