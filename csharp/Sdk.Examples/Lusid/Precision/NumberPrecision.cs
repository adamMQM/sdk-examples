using NUnit;
using NUnit.Framework;
using Lusid.Sdk;
using Lusid.Sdk.Extensions;
using Lusid.Sdk.Api;
using Lusid.Sdk.Client;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Json;
using Lusid.Sdk.Model;
using System.Collections.Generic;
using Sdk.Examples.Lusid.Sample;
using System.Threading;

namespace Sdk.Examples.Lusid.Precision.NumberPrecision
{

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public MockHttpMessageHandler(HttpResponseMessage response) => this.response = response;
        public HttpResponseMessage response;
        public CreateTransactionPortfolioRequest requestContents { get; set; }
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requestContents = request.Content.ReadFromJsonAsync<CreateTransactionPortfolioRequest>().Result;
            return response;
        }

        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requestContents = await request.Content.ReadFromJsonAsync<CreateTransactionPortfolioRequest>();
            return response;
        }
    }
    public class NumberPrecision
    {
        [Test]
        public void NumberPrecisionTest()
        {
            decimal testValue = 10.12345678901234567890123456789M;
            var expectedResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new Portfolio(id: new ResourceId(scope: "foo", code: "bar"), displayName: "foobar"))
            };
            var httpMessageHandlerMock = new MockHttpMessageHandler(expectedResponse);
            var apiClient = new ApiClient("http://example.com", (options) => httpMessageHandlerMock);
            var tpRequest = new CreateTransactionPortfolioRequest(displayName: "foo", code: "bar", baseCurrency: "GBP", properties: new Dictionary<string, Property>{
                    {"Number", new Property(key: "Number", value: new PropertyValue(metricValue: new MetricValue(value: testValue)))}
    });
            var api = new TransactionPortfoliosApi(apiClient, apiClient, new Configuration());
            api.CreatePortfolio(scope: "foo", tpRequest);
            var requestContent = httpMessageHandlerMock.requestContents;
            var numberProperty = requestContent.Properties["Number"].Value.MetricValue.Value;
            Assert.AreEqual(numberProperty, 10.123456789012345678901234568m);
        }
    }
}