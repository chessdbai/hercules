//-----------------------------------------------------------------------
// <copyright file="Function.cs" company="ChessDB.AI">
// MIT Licensed.
// </copyright>
//-----------------------------------------------------------------------

namespace Hercules.Triggers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.RuntimeSupport;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The entry point class for this Lambda function.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args">The input args.</param>
        /// <returns>An awaitable task.</returns>
        public static async Task Main(string[] args)
        {
            Func<Stream, ILambdaContext, Stream> func = FunctionHandler;
            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func);
            using var bootstrap = new LambdaBootstrap(handlerWrapper);
            await bootstrap.RunAsync();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        ///
        /// To use this handler to respond to an AWS event, reference the appropriate package from
        /// https://github.com/aws/aws-lambda-dotnet#events
        /// and change the string input parameter to the desired event type.
        /// </summary>
        /// <param name="input">The stream input to the Lambda function.</param>
        /// <param name="context">The Lambda execution context.</param>
        /// <returns>The output stream.</returns>
        public static Stream FunctionHandler(Stream input, ILambdaContext context)
        {
            string json;
            using (var reader = new StreamReader(input))
            {
                json = reader.ReadToEnd();
            }

            var response = RunTriggerAsync(json, context).Result;
            var responseBytes = Encoding.UTF8.GetBytes(response);
            return new MemoryStream(responseBytes);
        }

        private static async Task<string> RunTriggerAsync(string inputJson, ILambdaContext context)
        {
            dynamic jobj = JObject.Parse(inputJson);
            JObject response = new JObject();
            string eventType = jobj.triggerSource;
            if (eventType == "PostConfirmation_ConfirmSignUp")
            {
                string username = jobj.userName;
                response = await TriggerHandlers.HandlePostConfirmAsync(username, context);
            }
            else
            {
                Console.WriteLine($"Unknown event type: {eventType}");
                context.Logger.Log(inputJson);
            }

            var jAsObj = jobj as JObject;
            jAsObj["response"] = response;
            return JsonConvert.SerializeObject(jAsObj);
        }
    }
}
