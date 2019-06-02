// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoreBot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = CreateResponse(turnContext.Activity, welcomeCard);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                }
            }
        }

        // Create an attachment message response.
        private Activity CreateResponse(IActivity activity, Attachment attachment)
        {
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {

            // Construct a base URL for Image
            // To allow it to be found wherever the application is deployed
            //string strCurrentURL = @"D:\UPSProjectT&M\BoT POC\PoC\";

            string strCurrentURL = Assembly.GetExecutingAssembly().Location;
            string directoryPath = System.IO.Path.GetDirectoryName(strCurrentURL);
            string strImage = string.Format(@"{0}\{1}", directoryPath, @"\Images\ups-logo.jpg");
            List<CardImage> cardImages1 = new List<CardImage>();
            cardImages1.Add(new CardImage(url: strImage));
            CardAction btnAiHelpWebsite = new CardAction()
            {
                Type = "openUrl",
                Title = "inside.ups.com",
                Value = "http://airhome.inside.ups.com/AGP/"
            };
            // Finally create the Hero Card
            // adding the image and the CardAction
            HeroCard plCard1 = new HeroCard()
            {
                Title = "The UPS NRT Chatbot",
                Subtitle = "Hi Welcome! - please check reports: on-spot performance, post delivery performance",
                Images = cardImages1,
                Tap = btnAiHelpWebsite
            };

            return plCard1.ToAttachment();

            /*
            // combine path for cross platform support
            string[] paths = { ".", "Cards", "welcomeCard.json" };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
            */
        }
    }
}
