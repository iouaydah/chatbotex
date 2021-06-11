// Developed by Ismail Ouaydah for Ubility Customer ADX


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Json.Net;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// Middleware for translating text between the user and bot. Uses the Microsoft Translator Text API.
    /// </summary>
    public class TranslationMiddleware : IMiddleware
    {
        private readonly MicrosoftTranslator _translator;
        private readonly IStatePropertyAccessor<string> _languageStateProperty;
        private readonly BotState _conversationState;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationMiddleware"/> class.
        /// </summary>
        /// <param name="translator">Translator implementation to be used for text translation.</param>
        /// <param name="languageStateProperty">State property for current language.</param>
        public TranslationMiddleware(MicrosoftTranslator translator, ConversationState conversationState, UserState userState)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _conversationState = conversationState;

            if (userState == null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            _languageStateProperty = userState.CreateProperty<string>("LanguagePreference");
        }

        /// <summary>
        /// Processes an incoming activity, trying to pass the translate argument
        /// </summary>
        /// <param name="turnContext">Context object containing information for a single turn of conversation with a user.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var translate = await ShouldTranslateAsync(turnContext, cancellationToken);
            if (conversationData.forFAQ) translate = false;

            if (translate)
            {
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    //turnContext.Activity.Text = await _translator.TranslateAsync(turnContext.Activity.Text, TranslationSettings.DefaultLanguage, cancellationToken);
                    await TranslateMessageActivityAsync(turnContext.Activity.AsMessageActivity(), TranslationSettings.DefaultLanguage);
                }
            }

            //define the OnSendActivities handler will call TranslateMessageActivityAsync
            turnContext.OnSendActivities(async (newContext, activities, nextSend) =>
            {
                string userLanguage = await _languageStateProperty.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage) ?? TranslationSettings.DefaultLanguage;
                bool shouldTranslate = userLanguage != TranslationSettings.DefaultLanguage;
                if (conversationData.forFAQ) shouldTranslate = false;

                // Translate messages sent to the user on SendActivity, to user language
                if (shouldTranslate)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                    {
                        tasks.Add(TranslateMessageActivityAsync(currentActivity.AsMessageActivity(), userLanguage));
                    }

                    if (tasks.Any())
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                }

                return await nextSend();
            });

            //define the OnUpdateActivity handler will call TranslateMessageActivityAsync
            turnContext.OnUpdateActivity(async (newContext, activity, nextUpdate) =>
            {
                string userLanguage = await _languageStateProperty.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage) ?? TranslationSettings.DefaultLanguage;
                bool shouldTranslate = userLanguage != TranslationSettings.DefaultLanguage;

                // Translate messages sent to the user on update Activity to user language
                if (activity.Type == ActivityTypes.Message)
                {
                    if (shouldTranslate)
                    {
                        await TranslateMessageActivityAsync(activity.AsMessageActivity(), userLanguage);
                    }
                }

                return await nextUpdate();
            });

            await next(cancellationToken).ConfigureAwait(false);
        }

        // calls the MicrosoftTranslator.cs
        private async Task TranslateMessageActivityAsync(IMessageActivity activity, string targetLocale, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity.Type == ActivityTypes.Message & activity.Text!=null)
            {
                activity.Text = await _translator.TranslateAsync(activity.Text, targetLocale);
            }
            //added to check Attachements
            if (activity.Attachments != null)
            {
                if (activity.Type == ActivityTypes.Message & activity.Attachments.Count() > 0)
                {
                    //translate the content of the HeroCard
                    if (activity.Attachments[0].Content is HeroCard attachment)
                    {
                        //JObject result = JObject.FromObject(activity.Attachments[0].Content);
                        //var contentText= result["text"].ToString();
                        //var translatedText= await _translator.TranslateAsync(contentText, targetLocale);
                        //Attachment changed= activity.Attachments[0];
                    }

                }
            }
        }

        //returns false if the userLanguage is different than the Default Language
        private async Task<bool> ShouldTranslateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            string userLanguage = await _languageStateProperty.GetAsync(turnContext, () => TranslationSettings.DefaultLanguage, cancellationToken) ?? TranslationSettings.DefaultLanguage;
            return userLanguage != TranslationSettings.DefaultLanguage;
        }

        // below functions are for translating AdaptiveCards
        /**
          public async Task<object> TranslateAdaptiveCardAsync(object card, string targetLocale, CancellationToken cancellationToken = default)
          {
              var propertiesToTranslate = new[] { "text", "altText", "fallbackText", "title", "placeholder", "data" };

              var cardJObject = JObject.FromObject(card);
              var list = new List<(JContainer Container, object Key, string Text)>();

              void recurseThroughJObject(JObject jObject)
              {
                  var type = jObject["type"];
                  var parent = jObject.Parent;
                  var grandParent = parent?.Parent;
                  // value should be translated in facts and Input.Text, and ignored in Input.Date and Input.Time and Input.Toggle and Input.ChoiceSet and Input.Choice
                  var valueIsTranslatable = type?.Type == JTokenType.String && (string)type == "Input.Text"
                      || type == null && parent?.Type == JTokenType.Array && grandParent?.Type == JTokenType.Property && ((JProperty)grandParent)?.Name == "facts";

                  foreach (var key in ((IDictionary<string, JToken>)jObject).Keys)
                  {
                      switchOnJToken(jObject, key, propertiesToTranslate.Contains(key) || (key == "value" && valueIsTranslatable));
                  }
              }

              void switchOnJToken(JContainer jContainer, object key, bool shouldTranslate)
              {
                  var jToken = jContainer[key];

                  switch (jToken.Type)
                  {
                      case JTokenType.Object:

                          recurseThroughJObject((JObject)jToken);
                          break;

                      case JTokenType.Array:

                          var jArray = (JArray)jToken;
                          var shouldTranslateChild = key as string == "inlines";

                          for (int i = 0; i < jArray.Count; i++)
                          {
                              switchOnJToken(jArray, i, shouldTranslateChild);
                          }

                          break;

                      case JTokenType.String:

                          if (shouldTranslate)
                          {
                              // Store the text to translate as well as the JToken information to apply the translated text to
                              list.Add((jContainer, key, (string)jToken));
                          }

                          break;
                  }
              }

              recurseThroughJObject(cardJObject);
        
              // From Cognitive Services translation documentation:
              // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-csharp-translate
              var requestBody = JsonConvert.SerializeObject(list.Select(item => new { item.Text }));

              using (var request = new HttpRequestMessage())
              {
                  var uri = $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={targetLocale}";
                  request.Method = HttpMethod.Post;
                  request.RequestUri = new Uri(uri);
                  request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                  request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                  var response = await _client.SendAsync(request, cancellationToken);

                  response.EnsureSuccessStatusCode();

                  var responseBody = await response.Content.ReadAsStringAsync();
                  var result = JsonConvert.DeserializeObject<TranslatorResponse[]>(responseBody);

                  if (result == null)
                  {
                      return null;
                  }
              ========================================================
                  for (int i = 0; i < result.Length && i < list.Count; i++)
                  {
                      var item = list[i];
                      var translatedText = result[i]?.Translations?.FirstOrDefault()?.Text;

                      if (!string.IsNullOrWhiteSpace(translatedText))
                      {
                          // Modify each stored JToken with the translated text
                          item.Container[item.Key] = translatedText;
                      }
                  }
              ==========================================
  

                  // Return the modified JObject representing the Adaptive Card
                  return cardJObject;
              }

          }
      **/

    }
}
