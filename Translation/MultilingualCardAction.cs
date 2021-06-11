using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{

    public class MultilingualCardAction : CardAction
    {
        private readonly MicrosoftTranslator _translator;
        public IConfiguration configuration;

        private string _language;
        public MultilingualCardAction(string language)
        {
            _language = language;
            //_translator = new MicrosoftTranslator(configuration);
        }

        public string cardTitle
        {
            get
            {
                return this.Title;
            }

            set
            {
                this.Title = getTranslatedText(value).Result;
            }
        }

        async Task<string> getTranslatedText(string title)
        {
            return await _translator.TranslateAsync(title, _language);
        }
    }
}