# using cards: HeroCard, AdaptiveCard, ReceiptCard

Bot developed using Bot framework v4.0
Service1 Contact Us: present 2 options: Call Us / Visit branch which displays a HeroCard with a button to open map in google
Service2 Trade Summary: present 2 options: 
   Market Summary: prompt for Symbol , return results from an API call 
   Symbol Trading Summary: prompt for 
Service3 Reports (9): BalanceSheet, StatementofAccount, InvestorTrades, PledgeeShares, StockMovement, PledgeStatement, OwnershipTransfer, DividendInquiry, IPO
Service4 Fees Calculator: prompt for symbol, date
Service5 Dividend Calculator
Service FAQ connected to QnAMker KnowledgeBase
Service6 Charts & Reports Links

## Prerequisites

- appsetting.json contains the API Key for Azure Text Translator, the keys for English KnowledgeBase and the keys for Arabic KnowledgeBAse
- NuGet: AdaptiveCards, Microsoft.Bot.Builder.AI.QnA

## Architecture
DialogBot => MainDialog => ContactsDialog
						=> TradingDialog => DailySummaryDialog
										 => SymbolDetailsDialog
						=> ReportsDialog
						=> FeesDialog => GetServicesList () which calls an external api & returns a ServiceListModel (APIHelper.cs)
									  => GetServiceListCard (Cards.cs) displays a list of options
						=> Service5Dialog => 
                        => ChartLinksDialog
Other Dialogs:
DatePickerPrompt: Prompt is used to prompt for FromDate, ToDate, AsOfDate, companySelecttion, Broker Selection, Pledge Selection

Cards.cs: Displays the cards used for Input: CreateSymbolSelectionAttachement, CreateDynamicSelectionCardAttachment (this associate a json template with a json data)
          as well as the cards displaying the details: GetMapCard, GetDailySummaryChart, GetSymbolDetailsCard,

APIHelpers.cs: contains all the functions calling the APIs: GetDailySummary, GetSymbolDetails, GetReportFilter, GetReport ...

Models Data:
 -conversationData.cs: contains user selections, including reports params
 -APIResponseData.cs: contains API answers including reportfilters values (ReportFilterModel which saves Data as JObject)
 
QnAMaker Integration:
 - startup.cs: Addsingleton(botservices) and  Addsingleton(RootDialog)
 - BotServices.cs construct create 2 new QnAMaker: one for Arabic (Ar_QnAMakerService) and one for English (QnAMakerService)
 - RootDialog.cs AddDialog(QnAMakerBaseDialog) steps: CallGenerateAnswerAsync, CallTrain, CheckForMultiTurnPrompt, DisplayQnAResult
       + CallGenerateAnswerAsync: will call response= _services.Ar_QnAMakerService.GetAnswersRawAsync or _services.QnAMakerService.GetAnswersRawAsync
									and Ar_QnAMakerService.GetLowScoreVariation or QnAMakerService.GetLowScoreVariation
	   + CallTrain: Call Active Learning Train API, _services.Ar_QnAMakerService.CallTrainAsync or _services.QnAMakerService.CallTrainAsync
	   + DisplayQnAResult:
					reply = stepContext.Context.Activity.Text;
					stepContext.Context.SendActivityAsync(response.First().Answer or .NoAnswer
appsettings.json:QnAKnowledgebaseId, QnAAuthKey, QnAEndpointHostName retreived from qnamaker.ai/

## Json Templates
A- Input Selection Templates: brokerSelection Template, companySelectionTemplate, pledgeeSelectionTemplate, symbolSelectionTemplate...
 each of these templates has an Arabic version

 B- Data templates for dynamic Cards values: symboldata is a static list of 112 values,

 C- Presentation templates: symbolDetailsCard,  

## Translation
A mixed approach of using a Middleware to translate messages text, and manual approach to translate Adaptive Cards and Choices.
appsettings.json: TranslatorKey, Key1 in 

ArTranslator\Resource Management\Keys and Endpoints
\Translation\TranslationMiddleware.cs
