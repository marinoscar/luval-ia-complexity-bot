using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Luval.IA.Complexity.Dialogs
{
    public class ComplexityDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        

        public ComplexityDialog(ILogger<ComplexityDialog> logger) : base(nameof(ComplexityDialog))
        {
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if(stepContext.Result is string && !string.IsNullOrWhiteSpace(Convert.ToString(stepContext.Result)))
            {
                var result = Convert.ToString(stepContext.Result).ToLowerInvariant();
                switch (result)
                {
                    case "1":
                        await SendTextMessageAsync(stepContext, "Got A", cancellationToken);
                        await stepContext.RepromptDialogAsync(cancellationToken);
                        break;
                    case "2":
                        return await SendTextMessageAsync(stepContext, "Got B", cancellationToken);
                }
            }

            var didntUnderstandMessageText = $"Sorry, I didn't get that.";
            var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private async Task<ResourceResponse> SendTextMessageAsync(WaterfallStepContext stepContext, string message, CancellationToken cancellationToken)
        {
            var messageText = message;
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
        }
    }
}
