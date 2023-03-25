// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.IO;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;
using OrderPackagingWorkflow;
using RE.HelperFunctions;

namespace DemoApp
{
    public class OrderPackagingDemo
    {

        private int GetRandomNumber()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            return rnd.Next(1, 101);  // >= 1 and < 101      
        }
        public void Run()
        {
            int randomNumber = 0;

            #region load OrderPackagingInput.json as List<OrderPackagingInput> 
            var files1 = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderPackagingInput.json", SearchOption.AllDirectories);
            if (files1 == null || files1.Length == 0)
                throw new Exception("OrderPackagingInput.json not found.");

            var jsonFileData = File.ReadAllText(files1[0]);

            List<OrderPackagingInput> inputList = JsonConvert.DeserializeObject<List<OrderPackagingInput>>(jsonFileData);

            randomNumber = GetRandomNumber();

            #endregion

            #region load OrderPackagingHelper.json as input2
            var files2 = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderPackagingHelper.json", SearchOption.AllDirectories);
            if (files2 == null || files2.Length == 0)
                throw new Exception("OrderPackagingHelper.json not found.");

            var jsonFileData2 = File.ReadAllText(files2[0]);

            OrderPackagingHelper input2 = JsonConvert.DeserializeObject<OrderPackagingHelper>(jsonFileData2);
            #endregion

            #region load OrderPackagingDynamic.json as input3
            var files3 = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderPackagingDynamic.json", SearchOption.AllDirectories);
            if (files3 == null || files3.Length == 0)
                throw new Exception("OrderPackagingDynamic.json not found.");

            JObject jObject = JObject.Parse(File.ReadAllText(files3[0]));

            foreach (KeyValuePair<string, JToken> kvp in jObject)
            {
                Console.WriteLine(kvp.Key + " " + kvp.Value);
            }

            #endregion

            Console.WriteLine($"Running {nameof(OrderPackagingDemo)}....");

            #region Load Workflow file
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(),
                "OrderPackagingWorkflows.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
                throw new Exception("Rules not found.");

            var fileData = File.ReadAllText(files[0]);
            var workflow = JsonConvert.DeserializeObject<List<Workflow>>(fileData);
            #endregion

            var reSettingsWithCustomTypes = new ReSettings { CustomTypes = new Type[] { typeof(REUtils) } };
            var bre = new RulesEngine.RulesEngine(workflow.ToArray(), reSettingsWithCustomTypes);

            foreach (OrderPackagingInput input1 in inputList)
            {
                List<RuleResultTree> resultList;
                string location = "";
                bool status;


                (status, location, resultList) = ProcessBRE(input1, input2, jObject, bre);

                Console.WriteLine(location);
                PrintExceptions(resultList);
                PrintRuleResultList(resultList);
            }
        }

        public record struct RuleExceptionMessage
        {
            public string RuleName { get; set; }
            public string ExceptionMessage { get; set; }
        }

        private static void PrintRuleResultList(List<RuleResultTree> resultList)
        {
            Console.WriteLine("====");
            int ruleNumber = 1;
            foreach (RuleResultTree rrt in resultList)
            {
                if (rrt.IsSuccess)
                {
                    string message =
                    String.Format("IsSuccess: {0}", rrt.IsSuccess) + ", " +
                    String.Format("RuleNumber: {0}", ruleNumber) + ", " +
                    String.Format("RuleName: {0}", rrt.Rule.RuleName) + ", " +
                    String.Format("Expression: {0}", rrt.Rule.Expression) + ", " +
                    String.Format("SuccessEvent: {0}", rrt.Rule.SuccessEvent);
                    Console.WriteLine(message);
                    Console.WriteLine("");
                }
                ruleNumber++;
            }
            ruleNumber = 1;
            foreach (RuleResultTree rrt in resultList)
            {
                if (!rrt.IsSuccess)
                {
                    string message =
                    String.Format("IsSuccess: {0}", rrt.IsSuccess) + ", " +
                  String.Format("RuleNumber: {0}", ruleNumber) + ", " +
                  String.Format("RuleName: {0}", rrt.Rule.RuleName) + ", " +
                  String.Format("Expression: {0}", rrt.Rule.Expression) + ", " +
                  String.Format("SuccessEvent: {0}", rrt.Rule.SuccessEvent);
                    Console.WriteLine(message);
                    Console.WriteLine("");
                }
                ruleNumber++;
            }
            Console.WriteLine("====");
        }
        private static void PrintExceptions(List<RuleResultTree> resultList)
        {
            List<string> messages = GetExceptionMessages(resultList);

            if (messages.Count > 0)
            {
                string json = JsonConvert.SerializeObject(messages);
                Console.WriteLine(json);
            }
        }

        private static List<string> GetExceptionMessages(List<RuleResultTree> resultList)
        {
            List<string> messages = new List<string>();
            foreach (RuleResultTree rrt in resultList)
            {
                if (rrt.ExceptionMessage.Length > 0)
                {
                    string substring = rrt.ExceptionMessage.Substring(rrt.ExceptionMessage.IndexOf(":") + 1);
                    if (messages.Contains(substring))
                    {
                        ;
                    }
                    else
                    {
                        messages.Add(substring);
                    }
                }
            }
            return messages;
        }

        private static (bool, string, List<RuleResultTree>) ProcessBRE(
          OrderPackagingInput OrderPackagingInput
        , OrderPackagingHelper OrderPackagingHelper
        , JObject jObject
        , RulesEngine.RulesEngine bre)
        {
            var converter = new ExpandoObjectConverter();

            dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(jObject.ToString(), converter);

            List<RuleResultTree> resultList = new List<RuleResultTree>();
            string location = "No packaging was found";

            var rp1 = new RuleParameter("order", OrderPackagingInput);
            var rp2 = new RuleParameter("packaging", OrderPackagingHelper);
            var rp3 = new RuleParameter("dynamic", input3);

            resultList = bre.ExecuteAllRulesAsync(OrderPackagingInput.PSGName, rp1, rp2, rp3).Result;

            // If any exception messages, fail the entire rule

            bool status = false;

            resultList.OnSuccess((eventName) => {
                location = $"{OrderPackagingInput.ShipToState} Order routed to {eventName}.  {OrderPackagingInput.RandomNumber}";
                status = true;
            });

            resultList.OnFail(() => {
                location = $"{OrderPackagingInput.ShipToState} Order was not routed";
                status = false;
            });

            return (status, location, resultList);
        }
    }
}
