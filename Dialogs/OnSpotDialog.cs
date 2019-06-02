using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Bot.Builder;
using System.Threading;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.BotBuilderSamples;
using CoreBot.DataService;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class OnSpotDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;

        public OnSpotDialog()
            : base(nameof(OnSpotDialog))
        {

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GeoStepAsync,
                DateStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> GeoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var OnSpotPerformanceDetails = (OnSpotPerformance)stepContext.Options;

            if (OnSpotPerformanceDetails.Geo == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Which region you want to see?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(OnSpotPerformanceDetails.Geo, cancellationToken);
            }
        }

        
        private async Task<DialogTurnResult> DateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var OnSpotPerformanceDetails = (OnSpotPerformance)stepContext.Options;
            OnSpotPerformanceDetails.Geo = (string)stepContext.Result;

            if (OnSpotPerformanceDetails.Date == null || IsAmbiguous(OnSpotPerformanceDetails.Date))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), OnSpotPerformanceDetails.Date, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(OnSpotPerformanceDetails.Date, cancellationToken);
            }
        }


        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled or the user failed to confirm, the Result here will be null.
            if (stepContext.Result != null)
            {
                var result = (OnSpotPerformance)stepContext.Options;

                result.Date = (string)stepContext.Result;
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.
                var date = new TimexProperty(result.Date);
                var travelDateMsg = date.ToNaturalLanguage(DateTime.Now);

                // string value = DataAPI.GetOnSpotPerformanceData(result.Geo, travelDateMsg);

                string value = "100";

                var msg = $"You got data for {result.Geo}, {travelDateMsg} : {value}";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg, msg, null), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you."), cancellationToken);
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }

    }
}
