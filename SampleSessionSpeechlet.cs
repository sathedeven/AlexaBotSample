﻿using AlexaSkillsKit.Authentication;
using AlexaSkillsKit.Json;
using AlexaSkillsKit.Slu;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;
using Microsoft.Bot.Connector.DirectLine;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SimpleEchoBot
{
    public class SampleSessionSpeechlet : Speechlet
    {

        private static Logger Log = LogManager.GetCurrentClassLogger();


        // Bot integration point. Should be in a config file.
        private static string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static string botId = ConfigurationManager.AppSettings["BotName"];
        private static string fromUser = ConfigurationManager.AppSettings["FromUser"];

        public override void OnSessionStarted(SessionStartedRequest request, Session session)
        {
            //Log.Info("OnSessionStarted requestId={0}, sessionId={1}", request.RequestId, session.SessionId);
        }


        public override SpeechletResponse OnLaunch(LaunchRequest request, Session session)
        {
            //Log.Info("OnLaunch requestId={0}, sessionId={1}", request.RequestId, session.SessionId);
            return GetWelcomeResponse();
        }

        public override bool OnRequestValidation(SpeechletRequestValidationResult result, DateTime referenceTimeUtc, SpeechletRequestEnvelope requestEnvelope)
        {
            //if (requestEnvelope?.Session?.Application?.Id?.Equals("<your Alexa skill's application id here>") == false)
            //{
            //    //WebApiApplication.telemetry.TrackEvent("Request envelope does not contain the appid");
            //    return false;
            //}
            return base.OnRequestValidation(0, referenceTimeUtc, requestEnvelope);
        }


        public override SpeechletResponse OnIntent(IntentRequest request, Session session)
        {
            //Log.Info("OnIntent requestId={0}, sessionId={1}", request.RequestId, session.SessionId);
            string result = "";
            // Get intent from the request object.
            Intent intent = request.Intent;

            string intentName = (intent != null) ? intent.Name : null;
            string utterance = "";
            // Utterance pass through use this line

            utterance = request.Intent.Slots["phraseText"].Value;
            // Where using Alexa Intent and Slot definitions use this
            //string utterance = String.Format("{0} {1}", intentName, request.Intent.Slots["<your slot type>"].Value);

            Log.Info("utterance={0}, Secret={1}, BotId={2}", utterance, directLineSecret, botId);
            try
            {
                // Integrate with the bot service
                // Direct Line
                var client = new DirectLineClient(directLineSecret);
                var conversation = client.Conversations.StartConversation(); // should be Async
                string botresponse = null;
                var userId = session.User.Id;

                if (utterance != null && !string.IsNullOrEmpty(utterance))
                {
                    Activity message = new Activity
                    {

                        From = new ChannelAccount(fromUser),
                        Text = utterance,
                        Type = ActivityTypes.Message

                    };

                    client.Conversations.PostActivity(conversation.ConversationId, message);

                    result = ReadBotMessages(client, conversation.ConversationId, botresponse);

                    Log.Info("Result utterance={0}, result={1}", utterance, result);
                }
            }
            catch (Exception ex)
            {
                return BuildSpeechletResponse("error", "An error occurred, please try again", true);
            }
            return BuildSpeechletResponse("spoken", result, false);
            //return BuildSpeechletResponse("not understood", "I'm sorry I didn't understand", true);

        }

        private string ReadBotMessages(DirectLineClient client, string conversationId, string watermark)
        {

            ActivitySet messages = client.Conversations.GetActivities(conversationId, watermark);
            watermark = messages?.Watermark;

            var messagesFromBotText = from x in messages.Activities
                                      where x.From.Id == botId
                                      select x;

            string response = "";
            foreach (Activity activity in messagesFromBotText)
            {
                response = response + activity.Text + " ";
            }
            return response;
        }

        public override void OnSessionEnded(SessionEndedRequest request, Session session)
        {
            Log.Info("OnSessionEnded requestId={0}, sessionId={1}", request.RequestId, session.SessionId);
        }

        /**
         * Creates and returns a {@code SpeechletResponse} with a welcome message.
         * 
         * @return SpeechletResponse spoken and visual welcome message
         */
        private SpeechletResponse GetWelcomeResponse()
        {
            // Create the welcome message.
            string speechOutput =
                "Welcome to the Alexa AppKit session sample app modified to connect to Microsoft Bot Framework via the Direct Line Connector";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse("Welcome", speechOutput, false);
        }

        /**
         * Creates and returns the visual and spoken response with shouldEndSession flag
         * 
         * @param title
         *            title for the companion application home card
         * @param output
         *            output content for speech and companion application home card
         * @param shouldEndSession
         *            should the session be closed
         * @return SpeechletResponse spoken and visual response for the given input
         */
        private SpeechletResponse BuildSpeechletResponse(string title, string output, bool shouldEndSession)
        {
            // Create the Simple card content.
            SimpleCard card = new SimpleCard();
            card.Title = title; // String.Format("SessionSpeechlet - {0}", title);
            //card.Subtitle = String.Format("SessionSpeechlet");
            card.Content = output; // String.Format("SessionSpeechlet - {0}", output);

            // Create the plain text output.
            PlainTextOutputSpeech speech = new PlainTextOutputSpeech();
            speech.Text = output;

            // Create the speechlet response.
            SpeechletResponse response = new SpeechletResponse();
            response.ShouldEndSession = shouldEndSession;
            response.OutputSpeech = speech;
            response.Card = card;
            return response;
        }

    }
}