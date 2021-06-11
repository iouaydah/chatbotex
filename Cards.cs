// Developed by Ismail Ouaydah for Ubility Customer ADX
// namespace BotBuilderSamples

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AdaptiveCards;
using AdaptiveCards.Templating;

namespace Microsoft.BotBuilderSamples
{
    public static class Cards
    {
        //service1 card
        public static HeroCard GetMapCard(string title, string text, string buttontext, string imgurl)
        {
            // initialize the HeroCard with returned results: a Title and a picture
            var heroCard = new HeroCard
            {
                Title = title,
                Text = text,
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, buttontext, value: imgurl) },

            };
            return heroCard;
        }

        //called by service2.1: HeroCard Market Summary Displays a picture based on date
        public static HeroCard GetDailySummaryChartCard(APIResponseListstModel<DailySummaryAPIData> dailySummaryModel, string lang)
        {
            string button = (lang == "ar") ? "افتح المتصفح" : "Open in Browser";
            // initialize the HeroCard with returned results: a Title and a picture
            var heroCard = new HeroCard
            {
                Title = dailySummaryModel.Data[0].Title,
                Images = new List<CardImage> { new CardImage(dailySummaryModel.Data[0].URL) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, button, value: dailySummaryModel.Data[0].URL) },
            };
            return heroCard;
        }

        //called by service2.2: Symbol Trading Summary with button for more details
        public static Attachment GetSymbolDetailsCard(APIResponseListstModel<SymbolDetailsAPIData> symbol, string lang, string date)
        {
            // combine path for cross platform support
            string template;
            template = (lang == "ar") ? "symbolDetailsCardAR.json" : "symbolDetailsCard.json";

            string[] paths={ ".", "Resources", template };
            var cardJson = File.ReadAllText(Path.Combine(paths));
            var card = AdaptiveCard.FromJson(cardJson).Card;
            var title = (card.Body[0] as AdaptiveTextBlock);
            
            var factSet = (card.Body[1] as AdaptiveFactSet);
            var actionSet = (card.Body[2] as AdaptiveActionSet);

            var asOfDate = (factSet.Facts[0] as AdaptiveFact);
            asOfDate.Value = date;

            //var englishName = (factSet.Facts[1] as AdaptiveFact);
            //englishName.Value = symbol.Data[0].EnglishName;

            //var arabicName = (factSet.Facts[2] as AdaptiveFact);
            //arabicName.Value = symbol.Data[0].ArabicName;

            string subtitle = (lang == "ar") ? symbol.Data[0].ArabicName : symbol.Data[0].EnglishName;
            title.Text = subtitle+ " "+ title.Text ;

            var closePercent = (factSet.Facts[1] as AdaptiveFact);
            closePercent.Value = symbol.Data[0].CLOSE_PERCENT_CHANGE.ToString("0.00");

            var prevClosePrice = (factSet.Facts[2] as AdaptiveFact);
            prevClosePrice.Value = symbol.Data[0].PREV_CLOSE_PRICE.ToString("0.00");

            //var eqSymbol = (factSet.Facts[5] as AdaptiveFact);
            //eqSymbol.Value = symbol.Data[0].EQSymbol;

            var tradingSymbol = (factSet.Facts[3] as AdaptiveFact);
            tradingSymbol.Value = symbol.Data[0].TradingSymbol;

            //var DSymbol = (factSet.Facts[7] as AdaptiveFact);
            //DSymbol.Value = symbol.Data[0].DSYMBOL;

            var tradeVolume = (factSet.Facts[4] as AdaptiveFact);
            tradeVolume.Value = symbol.Data[0].TRADE_VOLUME.ToString("N0");

            var tradeValue = (factSet.Facts[5] as AdaptiveFact);
            tradeValue.Value = symbol.Data[0].TRADE_VALUE.ToString("N0");

            var tradesCount = (factSet.Facts[6] as AdaptiveFact);
            tradesCount.Value = symbol.Data[0].TRADES_COUNT.ToString("N0");

            var highPrice = (factSet.Facts[7] as AdaptiveFact);
            highPrice.Value = symbol.Data[0].HIGH_PRICE.ToString("0.00");

            var lowPrice = (factSet.Facts[8] as AdaptiveFact);
            lowPrice.Value = symbol.Data[0].LOW_PRICE.ToString("0.00");

            var openPrice = (factSet.Facts[9] as AdaptiveFact);
            openPrice.Value = symbol.Data[0].OPEN_PRICE.ToString("0.00");

            var closePrice = (factSet.Facts[10] as AdaptiveFact);
            closePrice.Value = symbol.Data[0].CLOSE_PRICE.ToString("0.00"); 

            var lastTraded = (factSet.Facts[11] as AdaptiveFact);

            //Parse Date: "/Date(1547150400000+0400)/"
            string NotAv = (lang == "ar") ? "غير متوفر" : "NA";
            string passedarg = symbol.Data[0].LastTraded;
            string lasttradedate = (passedarg !=null)? APIRestHelper.parsedate(passedarg): NotAv;

            lastTraded.Value = lasttradedate;

            var moreDetails = (actionSet.Actions[0] as AdaptiveOpenUrlAction);
            moreDetails.Url = new Uri(symbol.Data[0].URL);

            var symbolCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };

            return symbolCardAttachment;
        }

        //called by Reports Service to display Selection Attachement: Company, Broker, Pledgee with dynamic values from API call
        public static Attachment CreateDynamicAdaptiveCardAttachment(JObject dataJson, string template)
        {
            // combine path for cross platform support
            string[] templatepaths = { ".", "Resources", template };
            var templateJson = File.ReadAllText(Path.Combine(templatepaths));

            Attachment attachment = new Attachment();
            if (dataJson != null)
            {
                string datastring = dataJson.ToString();

                AdaptiveTransformer transformer = new AdaptiveTransformer();
                string cardJson = transformer.Transform(templateJson, datastring);

                attachment.ContentType = "application/vnd.microsoft.card.adaptive";
                attachment.Content = JsonConvert.DeserializeObject(cardJson);
            }
            else
            {
                //very unlikely to have dataJson null
                var imagePath = Path.Combine(Environment.CurrentDirectory, @"Resources\nodata.png");
                var imageData = Convert.ToBase64String(File.ReadAllBytes(imagePath));

                attachment.Name = @"Resources\nodata.png";
                attachment.ContentType = "image/png";
                attachment.ContentUrl = $"data:image/png;base64,{imageData}";
            }

            return attachment;
        }

        //called by Reports Service: return a Card Attachment for displaying report (pdf) link
        public static Attachment GetReportCard(String reportName, ReportResponseModel report)
        {
            // read the Title and URL from the report Model 
            Attachment attachment = new Attachment()
            {
                Name = "ADX Report: "+reportName, //report.Title,   //+"\n("+report.URL+")",
                ContentType = "application/pdf",
            };
            if (report.Data == null) //if (report.URL == null)
            {
                attachment.Name = "No Data Available";
                attachment.ContentUrl = "http://";
            }
            else
            {
                attachment.ContentUrl = $"data:application/pdf;base64,{report.Data.Data}";
            }

            return attachment;
        }

        //called by Fees Calculator To display fees of selected service, symbol and date
        public static Attachment GetServiceFeeAdaptiveCard(APIResponseListstModel<ServiceFeeAPIData> serviceFee, string symbol, string service, string quantity, string lang)
        {
            //unable to display symbol name and service name. Code values are displayed instead
            string passedarg = serviceFee.Data[0].ClosingDate;
            string date = APIRestHelper.parsedate(passedarg); //Parse Date: "/Date(1547150400000+0400)/"

            string template;
            template = (lang == "ar") ? "ServiceFeesCardAR1.json" : "ServiceFeesCard.json";
            int colindex = 1; // (lang == "ar") ? 0 : 1;
            int itemindex = 0;  // (lang == "ar") ? 0 : 0;

            string[] paths = { ".", "Resources", template };
            var cardJson = File.ReadAllText(Path.Combine(paths));
            var card = AdaptiveCard.FromJson(cardJson).Card;
            var titlecontainer = (card.Body[0] as AdaptiveContainer);
            var titleitem= (titlecontainer.Items[0] as AdaptiveTextBlock);

            string subtitle = (lang == "ar") ? symbol : symbol;
            string titletext = (lang == "ar") ? titleitem.Text + " " + service + " : " + subtitle: service + " " + titleitem.Text + " " + subtitle;
            titleitem.Text = titletext;

            var valuescontainer = (card.Body[1] as AdaptiveContainer);

            var closingdatecolumnset = valuescontainer.Items[0] as AdaptiveColumnSet;
            var closingdatecolumn = closingdatecolumnset.Columns[colindex] as AdaptiveColumn;
            var closingdate = closingdatecolumn.Items[itemindex] as AdaptiveTextBlock;
            closingdate.Text = date;

            var sharescolumnset = valuescontainer.Items[1] as AdaptiveColumnSet;
            var sharescolumn = sharescolumnset.Columns[colindex] as AdaptiveColumn;
            var sharesqty = sharescolumn.Items[itemindex] as AdaptiveTextBlock;
            sharesqty.Text = quantity;

            var closingpricecolumnset = valuescontainer.Items[2] as AdaptiveColumnSet;
            var closingpricecolumn = closingpricecolumnset.Columns[colindex] as AdaptiveColumn;
            var closingprice = closingpricecolumn.Items[itemindex] as AdaptiveTextBlock; 
            closingprice.Text = serviceFee.Data[0].ClosingPrice.ToString(); // display it as received

            var servicefeecolumnset = valuescontainer.Items[3] as AdaptiveColumnSet;
            var servicefeecolumn = servicefeecolumnset.Columns[colindex] as AdaptiveColumn;
            var servicefee = servicefeecolumn.Items[itemindex] as AdaptiveTextBlock;
            servicefee.Text = (serviceFee.Data[0].Total - serviceFee.Data[0].VAT).ToString("N2");

            var VATcolumnset = valuescontainer.Items[4] as AdaptiveColumnSet;
            var VATcolumn = VATcolumnset.Columns[colindex] as AdaptiveColumn;
            var VAT = VATcolumn.Items[itemindex] as AdaptiveTextBlock;
            VAT.Text = serviceFee.Data[0].VAT.ToString("N2");

            var totalcolumnset = valuescontainer.Items[5] as AdaptiveColumnSet;
            var totalcolumn = totalcolumnset.Columns[colindex] as AdaptiveColumn;
            var total = totalcolumn.Items[itemindex] as AdaptiveTextBlock;
            total.Text = serviceFee.Data[0].Total.ToString("N2");

            var serviceFeeCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = card,
            };

            return serviceFeeCardAttachment;
        }
        
        //called by Service5 API 2.2 of Anonymous user
        public static ReceiptCard GetDividendCard(Service5CalculateData dividend, string lang, string symbol, string year, int qty)
        {
            // might need to be changed because the returned data is not fixed.
            string label;
            string cashlabel;
            string qtyLabel;
            string symbolLabel;
            string yearLabel;
            symbolLabel = (lang == "ar") ? "الشركة" : "Company:";
            yearLabel = (lang == "ar") ? "السنة" : "Year:";
            qtyLabel = (lang == "ar") ? "الكمية" : "Quantity:";
            label =  (lang == "ar")? "الارباح النقدية" : "Cash Dividend (AED):";
            cashlabel = (lang == "ar") ? "الارباح السهمية " : "Bonus Shares:";
            var dividendval = dividend.Dividend.ToString("N0");

            //initialize Card to present the results: ClosingDate 	ClosingPrice Total
            var receiptCard = new ReceiptCard
            {
                Title = "Calculation results for " + symbol,
                Facts = new List<Fact> { new Fact(yearLabel, year ),
                                         new Fact(qtyLabel, qty.ToString("N0") ),
                                         new Fact(label, dividendval ),
                                         new Fact(cashlabel, dividend.Cash.ToString("N0"))
                                        }

            };

            return receiptCard;

        }

        //used for date picker in all Dialogs
        public static Attachment CreateDateAdaptiveCardAttachment(string mindate, string maxdate, string button="Submit")
        {
            // called by all Market Summary, Fees Calculcator and Report Services
            string[] paths = { ".", "Resources", "adaptiveDateCard.json" };
            var templateJson = File.ReadAllText(Path.Combine(paths));

            //Create an Adaptive Card with max date and mnin dates received from caller
            var dataJson = "{\"items\": [{\"mindate\":\"" + mindate + "\",\"maxdate\":\"" + maxdate + "\",\"button\":\"" + button + "\"}]}  ";

            AdaptiveTransformer transformer = new AdaptiveTransformer();
            string cardJson = transformer.Transform(templateJson, dataJson);

            var dateCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson)
            };

            return dateCardAttachment;
        }

        // creates from template and objectslist an AdaptiveCard attachement that can be sent in messages or added to a prompt
        //used  for Showing Interims in Service5Dialog and SymbolsSelection in Service5Dialog, SymbolDetailsDialog and FeesDialog
        public static Attachment CreatefromListAdaptiveCardAttachment<T>(List<T> objectslist, string template)
        {
            // combine path for cross platform support
            string[] templatepaths = { ".", "Resources", template };
            var templateJson = File.ReadAllText(Path.Combine(templatepaths));

            string dataJson = "{\"Data\":" + JsonConvert.SerializeObject(objectslist, Formatting.Indented) + "}";
            AdaptiveTransformer transformer = new AdaptiveTransformer();
            string cardJson = transformer.Transform(templateJson, dataJson);

            // create an AdaptiveCard Attachement with the list
            var selectionCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson)
            };

            return selectionCardAttachment;
        }

        //Adaptive Card with Static content
        public static Attachment CreateAdaptiveCardAttachment(string template)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Resources", template };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

    }
}
