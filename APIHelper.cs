// Developed by Ismail Ouaydah for Ubility Customer ADX
// namespace BotBuilderSamples

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Bot.Schema;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public static class APIRestHelper
    {
        public static HttpClient APIclient { get; set; }

        public static Attachment loginAttachment()
        {

            Attachment loginattachment = Cards.CreateAdaptiveCardAttachment("loginCard.json");
            return loginattachment;
        }

        public static void InitializeClient()
        {
            APIclient = new HttpClient();
            APIclient.DefaultRequestHeaders.Accept.Clear();
            APIclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //add Authorization and Channel to the Header
            APIclient.DefaultRequestHeaders.Add("Channel", "Mobile"); //by default, Portal will change this in channeldata
        }

        public static void PrepareHeader(ConversationData data)
        {
            string channel = "Portal";
            if (data.ChannelId == "Mobile") channel = "Mobile";
            if (data.ChannelId == "KIOSK") channel = "KIOSK";

            //check Authorization value in the Header it should be the same as the one received in the request (token), if not replace it
            if (APIRestHelper.APIclient.DefaultRequestHeaders.Contains("Authorization"))
            {
                // Check if user has changed... TODO--below code is always executing (only difference I can see between the strings is the quote)
                if (!APIRestHelper.APIclient.DefaultRequestHeaders.GetValues("Authorization").ToString().Equals(data.Token))
                {
                    APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Authorization");
                    APIRestHelper.APIclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", data.Token);
                }

            }
            else
            {
                APIRestHelper.APIclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", data.Token);
            }

            if (APIRestHelper.APIclient.DefaultRequestHeaders.Contains("Channel"))
            {
                // Check if user has changed... TODO--below code is always executing (only difference I can see between the strings is the quote
                if (!APIRestHelper.APIclient.DefaultRequestHeaders.GetValues("Channel").ToString().Equals(channel))
                {
                    APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Channel");
                    APIRestHelper.APIclient.DefaultRequestHeaders.TryAddWithoutValidation("Channel", channel);
                }

            }
            else
            {
                APIRestHelper.APIclient.DefaultRequestHeaders.TryAddWithoutValidation("Channel", channel);
            }

        }

        //returning "Date/secondssinceEpoch/" from actual date
        public static string dateformater(DateTime date, bool milliseconds=true)
        {
            TimeSpan t = date - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            string closeend = ")/\"";
            if (milliseconds) closeend = "000)/\"";
            string datestring = "\"/Date(" + secondsSinceEpoch.ToString() + closeend;
            return datestring;
        }

        public static string dateformater(string date, bool milliseconds = false)
        {
            DateTime oDate = DateTime.Parse(date);
            string datestring = dateformater(oDate, milliseconds);
            return datestring;
        }

        public static string formatdate(string date, string culturestr = "fr-FR")
        {
            //date is coming in a different format
            DateTime oDate = DateTime.Parse(date);
            CultureInfo culture = new CultureInfo(culturestr);
            string datestring = oDate.ToString("d", culture);
            return datestring;
        }

        //returning shortdate from unix milliseconds
        public static string parsedate(string passedarg)
        {
            int index = passedarg.IndexOf("(");
            string Epoch = passedarg.Substring(index + 1, 13);
            long MillisecondsSinceEpoch = long.Parse(Epoch);
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(MillisecondsSinceEpoch);
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            CultureInfo culture = new CultureInfo("fr-FR");
            //string date = dateTime.ToShortDateString();
            string date = dateTime.ToString("d", culture); 

            return date;
        }
    }

    public class APIProcessor
    {
        //API for Trading/ Market Summary Service
        public static async Task<APIResponseListstModel<DailySummaryAPIData>> GetDailySummary(DateTime date)
        {
            //compose raw data with the argument passed from the Dialog
            string datestring = APIRestHelper.dateformater(date);

            string rawData = "{\"SenderInfo\":\"chatbot\",\"SummaryDate\":" + datestring + "}";
            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            if (APIRestHelper.APIclient.DefaultRequestHeaders.Contains("Authorization")) APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Authorization");
            if (APIRestHelper.APIclient.DefaultRequestHeaders.Contains("Channel")) APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Channel");

            string URL = "https://chatbotservices.adx.ae/Services.svc/GetDailySummaryChart";
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<DailySummaryAPIData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<DailySummaryAPIData>>();
                    if (!apiResponse.Status)
                    {
                        throw new Exception("API GetDailySummary" + apiResponse.Message);

                    }
                    else
                    {
                        return apiResponse;
                    }

                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);
                }
            }
        }

        //API for Trading /Symbol Details Service
        public static async Task<APIResponseListstModel<SymbolDetailsAPIData>> GetSymbolDetails(string EqSymbol, string date)
        {
            string URL = "https://chatbotservices.adx.ae/Services.svc/GetCurrentSymbolDetails";

            //compose raw data with the argument passed
            string datestring = APIRestHelper.dateformater(date, true);
            string rawData = "{\"SendorInfo\":\"chatbot\",\"EqSymbol\":\"" + EqSymbol + "\",\"SymbolDate\":" + datestring+"}";
            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            if (APIRestHelper.APIclient.DefaultRequestHeaders.Contains("Authorization")) APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Authorization");
            if (APIRestHelper.APIclient.DefaultRequestHeaders.Contains("Channel")) APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Channel");

            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<SymbolDetailsAPIData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<SymbolDetailsAPIData>>();
                    if (!apiResponse.Status)
                    {
                        throw new Exception("API GetSymbolDetails" + apiResponse.Message);

                    }
                    else
                    {
                        return apiResponse;
                    }
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //API for service3.1... reports filters
        //==========================================
        public static async Task<ReportFilterModel> GetReportFilters(string reportname, string token, ConversationData data)
        {
            //return reports filter (Listed security, Brokerage Firm and  based on the token
            string URL = "https://chatbotservices.adx.ae/ReportServices.svc";

            APIRestHelper.PrepareHeader(data);

            string asofdate = data.AsOfDate;
            string fromdate = data.FromDate;
            string todate = data.ToDate;

            //compose raw data fromDate, ToDate or AsofDate passed in conversationData in the argument
            string rawData = "{\"FromDate\":\"" + fromdate + "\",\"ToDate\":\"" + todate + "\"}";
            switch (reportname)
            {
                case "Balance Sheet Report":
                    rawData = "{\"AsOfDate\":\"" + asofdate + "\"}"; // json body for filter request is different than the rest
                    URL = URL + "/InvestorBalanceSheetReport"; 
                    break;
                case "Statement of Account Report":
                    URL = URL + "/InvestorStatementOfAccountReport"; 
                    break;
                case "Investor Trade Report":
                    URL = URL + "/InvestorTradeReport"; 
                    break;
                case "Pledged Shares Report": // does not have dynamic filters
                    break;
                case "Stock Movement Report":
                    URL = URL + "/StockMovementMasterReport";
                    break;
                case "Pledge Statement Report":
                    URL = URL + "/PledgeStatementMasterReport";
                    break;
                case "Ownership Transfer Report": // does not have dynamic filters
                    break;
                case "Divident Inquiry Report":
                case "Dividend Inquiry Report":
                    URL = URL + "/DividendInquiryReport"; 
                    break;
                case "Initial Public Offering Report": 
                    URL = URL + "/IPOReport"; 
                    break;
            }

            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");

            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                    if (responsemMsg.IsSuccessStatusCode)
                {
                    ReportFilterModel apiResponse = await responsemMsg.Content.ReadAsAsync<ReportFilterModel>();
 
                    if (!apiResponse.Status)
                    {
                        // Data=null, status=false
                        throw new Exception("API Report Filter" + apiResponse.Message);

                    }
                    else
                    {
                        return apiResponse;
                    }
                }
                else
                {
                    //API call return Unauthorized or Bad Request 400
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //API for service3.2... download different reports
        public static async Task<ReportResponseModel> GetReport(string reportname, string token, ConversationData data)
        {
            string URL = "https://chatbotservices.adx.ae/ReportServices.svc";
            /**
            string channel="Portal";
            if (data.ChannelId == "Mobile") channel = "Mobile";
            APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Channel");
            APIRestHelper.APIclient.DefaultRequestHeaders.TryAddWithoutValidation("Channel", channel);

            APIRestHelper.APIclient.DefaultRequestHeaders.Remove("Authorization");
            APIRestHelper.APIclient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
            **/
            string asofdate = data.AsOfDate;
            string fromdate = data.FromDate;
            string todate = data.ToDate;

            APIRestHelper.PrepareHeader(data);

            //compose raw data Params objects with the arguments passed and serialize as Json
            string rawData = "";
            switch (reportname)
            {
                case "Balance Sheet Report":
                    BalanceSheetParams balparam = new BalanceSheetParams();
                    balparam.AsOfDate = asofdate;
                    balparam.Brokers = (data.BrokerageFirm != null)?  new List<string> { data.BrokerageFirm }: new List<string>() ;
                    if (data.CompanyName != null) balparam.Symbols = new List<string> { data.CompanyName };
                    rawData = JsonConvert.SerializeObject(balparam); 
                    URL = URL + "/DownloadInvestorBalanceSheetReport"; 
                    break;
                case "Statement of Account Report":
                    StatementofAccountParams apiparam = new StatementofAccountParams();
                    apiparam.FromDate = fromdate;
                    apiparam.ToDate = todate;
                    apiparam.Brokers = (data.BrokerageFirm !=null)?  new List<string> { data.BrokerageFirm }: new List<string>();
                    apiparam.Symbols = (data.CompanyName !=null) ? new List<string> { data.CompanyName }: new List<string>();
                    rawData = JsonConvert.SerializeObject(apiparam);
                    URL = URL + "/DownloadInvestorStatementOfAccountReport";
                    break;
                case "Investor Trade Report":
                    StatementofAccountParams trdparam = new StatementofAccountParams();
                    trdparam.FromDate = fromdate;
                    trdparam.ToDate = todate;
                    trdparam.Brokers = (data.BrokerageFirm != null)?  new List<string> { data.BrokerageFirm }: new List<string>();
                    trdparam.Symbols = (data.CompanyName != null) ? new List<string> { data.CompanyName }: new List<string>();
                    rawData = JsonConvert.SerializeObject(trdparam);
                    URL = URL + "/DownloadInvestorTradeReport"; 
                    break;
                case "Pledged Shares Report":
                    rawData = "{\"AsOfDate\":\"" + asofdate + "\",\"Format\":0}";
                    URL = URL + "/DownloadPledgedSharesReport"; 
                    break;
                case "Stock Movement Report":
                    StockMvtParams stockparam = new StockMvtParams();
                    stockparam.FromDate = fromdate;
                    stockparam.ToDate = todate;
                    stockparam.ReportType = data.ReportType;
                    stockparam.Brokers = (data.BrokerageFirm != null)?  new List<string> { data.BrokerageFirm } : new List<string>();
                    stockparam.Symbols = (data.CompanyName != null) ? new List<string> { data.CompanyName }: new List<string>();
                    rawData = JsonConvert.SerializeObject(stockparam);
                    URL = URL + "/DownloadStockMovementReport";
                    break;
                case "Pledge Statement Report":
                    PldgeStatementParams pledgeparam = new PldgeStatementParams();
                    pledgeparam.FromDate = fromdate;
                    pledgeparam.ToDate = todate;
                    pledgeparam.TransactionType = data.TransactionType;
                    pledgeparam.Pledges = (data.PledgeeName != null) ? new List<string> { data.PledgeeName }: new List<string>();
                    pledgeparam.Symbols = (data.CompanyName != null) ? new List<string> { data.CompanyName }: new List<string>();
                    rawData = JsonConvert.SerializeObject(pledgeparam);
                    URL = URL + "/DownloadPledgeStatementReport"; 
                    break;
                case "Ownership Transfer Report":
                    OwnershipTransferParams ownerparam = new OwnershipTransferParams();
                    ownerparam.FromDate = fromdate;
                    ownerparam.ToDate = todate;
                    ownerparam.TransferType = data.TransferType;
                    ownerparam.TransferDirection = data.TransferDirection;
                    rawData = JsonConvert.SerializeObject(ownerparam);
                    URL = URL + "/DownloadOwnershipTransferReport";
                    break;
                case "Divident Inquiry Report": // under dev. need to check with service 5??
                    DividendInquiryParams divparam = new DividendInquiryParams();
                    divparam.FromDate = fromdate;
                    divparam.ToDate = todate;
                    divparam.DividendType = data.DividendType;
                    divparam.SymbolCodes = (data.CompanyName != null && data.CompanyName != "All") ? new List<string> { data.CompanyName }: new List<string>();
                    divparam.StatusCodes = (data.StatusCode != null && data.StatusCode != "All") ? new List<string> { data.StatusCode }: new List<string>(); 
                    rawData = JsonConvert.SerializeObject(divparam);
                    URL = URL + "/DownloadDividendInquiryReport"; 
                    break;
                case "Initial Public Offering Report":
                    IPOParams IPOparam = new IPOParams();
                    IPOparam.FromDate = fromdate;
                    IPOparam.ToDate = todate;
                    if (data.CompanyName != null) IPOparam.SymbolCodes = new List<string> { data.CompanyName }; 
                    rawData = JsonConvert.SerializeObject(IPOparam);
                    URL = URL + "/DownloadIPOReport"; 
                    break;
            }

            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            
            //Serialize the ReportResponseModel
            using (HttpResponseMessage response = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData)) 
            {
                if (response.IsSuccessStatusCode)
                {
                    //Read the Response
                    ReportResponseModel responseModel = await response.Content.ReadAsAsync<ReportResponseModel>();
                    if (!responseModel.Status) 
                    {
                        throw new Exception("Problem in getting Report" + responseModel.Message);
                    }
                    else {
                        return responseModel;
                    }
                }
                else
                {   
                    //API call return Unauthorized or Bad Request 400
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        //API.1 for Calculated Fees Services (service 4)
        //=============================================
        public static async Task<APIResponseListstModel<ServiceListAPIData>> GetServicesList(string lang)
        {
            string URL = "https://chatbotservices.adx.ae/Services.svc/GetServicesList";

            //no arguments passed
            int blang=(lang=="ar")? 1:2;
            string rawData = "{\"SenderInfo\":\"chatbot\",\"SortLanguage\":"+blang+"}";
            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<ServiceListAPIData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<ServiceListAPIData>>();
                    if (!apiResponse.Status)
                    {
                        throw new Exception("API GetDailySummary" + apiResponse.Message);

                    }
                    else
                    {
                        return apiResponse;
                    }
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //API.2 for Calculated Fees Services
        public static async Task<APIResponseListstModel<ServiceFeeAPIData>> GetServiceCalculatedFee(string symbol, string serviceID, DateTime closingDate, int shares)
        {
            string URL = "https://chatbotservices.adx.ae/Services.svc/GetServicesCalculatedFee";

            //compose raw data with arguments passed
            string datestring = APIRestHelper.dateformater(closingDate);

            string rawData = "{\"symbol\":\"" + symbol + "\",\"serviceID\":\"" + serviceID + "\",\"closingDate\":" + datestring + ",\"numberOfShares\" :\"" + shares.ToString() + "\"}";
            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<ServiceFeeAPIData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<ServiceFeeAPIData>>();
                    return apiResponse;
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //Service5 APIs...Dividend Inquiry ....
        //=============================================
        public static async Task<ReportFilterModel> GetStatusList(ConversationData data)
        {
            //return reports filter (Listed security, Brokerage Firm and  based on the token
            string URL = "https://chatbotservices.adx.ae/ReportServices.svc/DividendInquiryStatusList";

            // check if the user is loggedin data.Token
            APIRestHelper.PrepareHeader(data);

            HttpContent postData = new StringContent("", System.Text.Encoding.UTF8, "application/json");

            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    ReportFilterModel apiResponse = await responsemMsg.Content.ReadAsAsync<ReportFilterModel>();

                    if (!apiResponse.Status)
                    {
                        // Data=null, status=false
                        throw new Exception("API Status List" + apiResponse.Message);

                    }
                    else
                    {
                        return apiResponse;
                    }
                }
                else
                {
                    //API call return Unauthorized or Bad Request 400
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //API-0 for service5 returns list of all symbols available, also used by SymbolsDetailsDialog and FeesDialog
        public static async Task<APIResponseListstModel<Service5SymbolsData>> GetSymbolsList(String lang)
        {
            string URL = "https://chatbotservices.adx.ae/Services.svc/GetAllSymbols";

            //no arguments passed
            int blang = (lang == "ar") ? 1 : 2;
            string rawData = "{\"SenderInfo\":\"CRM\",\"SortLanguage\":" + blang +"}";
            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<Service5SymbolsData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<Service5SymbolsData>>();
                    return apiResponse;
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }
        //API-1 for service5 
        public static async Task<APIResponseListstModel<Service5CorporateActionsData>> GetCorporateActions(string symbol, string year)
        {
            //returns Interims

            string URL = "https://chatbotservices.adx.ae/Services.svc/GetCoperateActionsByYear";

            GetCoporateParams Corpparam = new GetCoporateParams();
            Corpparam.Symbol = symbol;
            Corpparam.Year = year;
            string rawData = JsonConvert.SerializeObject(Corpparam);

            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<Service5CorporateActionsData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<Service5CorporateActionsData>>();
                    return apiResponse;
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //Not Used- Changed Requirements API-2.1 for service5 for loggedin user returns multiple values
        public static async Task<APIResponseListstModel<Service5InvestorDividendData>> GetInvestorDividends(string symbol, string year, ConversationData data)
        {
            // Shows the logged in user Dividend cash/ bonus multiple times
            string URL = "https://chatbotservices.adx.ae/Services.svc/GetInvestordividend";

            //add Authorization
            APIRestHelper.PrepareHeader(data);

            //compose raw data with arguments passed
            GetInvestorDividendParams Invparam = new GetInvestorDividendParams();
            Invparam.EQSYMBOL = symbol;
            Invparam.Year = year;
            string rawData = JsonConvert.SerializeObject(Invparam);

            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    APIResponseListstModel<Service5InvestorDividendData> apiResponse = await responsemMsg.Content.ReadAsAsync<APIResponseListstModel<Service5InvestorDividendData>>();
                    return apiResponse;
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }

        //API-2.2 for service5 for anonymous user returns one value
        public static async Task<Service5CalculateModel> CalculateDividends(string symbol, string year, int qty)
        {
            //shows only one value Dividend:
            string URL = "https://chatbotservices.adx.ae/Services.svc/CalculateMydividend";

            //compose raw data with arguments passed
            CalculateMyDividendParams MyDivparam = new CalculateMyDividendParams();
            MyDivparam.EQSYMBOL = symbol;
            MyDivparam.Year = year;
            MyDivparam.QTY = qty;
            string rawData = JsonConvert.SerializeObject(MyDivparam);

            HttpContent postData = new StringContent(rawData, System.Text.Encoding.UTF8, "application/json");
            using (HttpResponseMessage responsemMsg = await APIRestHelper.APIclient.PostAsync(new Uri(URL), postData))
            {
                if (responsemMsg.IsSuccessStatusCode)
                {
                    Service5CalculateModel apiResponse = await responsemMsg.Content.ReadAsAsync<Service5CalculateModel>();
                    return apiResponse;
                }
                else
                {
                    throw new Exception(responsemMsg.ReasonPhrase);

                }
            }
        }
        
        
        
    }


}
