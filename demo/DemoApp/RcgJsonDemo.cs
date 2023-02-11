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
using OrderRoutingWorkflow;
using RE.HelperFunctions;

namespace DemoApp
{
  public class RcgJsonDemo
  {

    private int GetRandomNumber()
    {
      Random rnd = new Random(DateTime.Now.Millisecond);
      return rnd.Next(1, 101);  // >= 1 and < 101      
    }
    public void Run()
    {
      #region load OrderRoutingInput.json as input1
      var files1 = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderRoutingInput.json", SearchOption.AllDirectories);
      if (files1 == null || files1.Length == 0)
        throw new Exception("OrderRoutingInput.json not found.");

      var jsonFileData = File.ReadAllText(files1[0]);

      OrderRoutingInput input1 = JsonConvert.DeserializeObject<OrderRoutingInput>(jsonFileData);

      input1.RandomNumber = GetRandomNumber();


      #endregion

      #region load OrderRoutingHelper.json as input2
      var files2 = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderRoutingHelper.json", SearchOption.AllDirectories);
      if (files2 == null || files2.Length == 0)
        throw new Exception("OrderRoutingHelper.json not found.");

      var jsonFileData2 = File.ReadAllText(files2[0]);

      OrderRoutingHelper input2 = JsonConvert.DeserializeObject<OrderRoutingHelper>(jsonFileData2);
      #endregion

      #region get active locations
      var productCount = input1.ProductCodes.Count;

      input1.ActiveLocations = new List<string>();

      foreach (string val in input2.LocationNames)
      {

        var activeProductCount = input2.ActiveProducts
          .Where(x => x.LocationName == val)
          .Select(a => a.ProductCode)
          .Intersect(input1.ProductCodes).Count();

        if (productCount == activeProductCount)
        {
          input1.ActiveLocations.Add(val);
        }
      }

      #endregion

      #region load OrderRoutingDynamic.json as input3
      var files3 = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderRoutingDynamic.json", SearchOption.AllDirectories);
      if (files3 == null || files3.Length == 0)
        throw new Exception("OrderRoutingDynamic.json not found.");

      JObject jsonFileData3 = JObject.Parse(File.ReadAllText(files3[0]));

      foreach (KeyValuePair<string, JToken> kvp in jsonFileData3)
      {
        Console.WriteLine(kvp.Key + " " + kvp.Value);
      }

      var converter = new ExpandoObjectConverter();

      dynamic input3 = JsonConvert.DeserializeObject<ExpandoObject>(jsonFileData3.ToString(), converter);
      #endregion

      bool boolvalue = input3.ProductsToForceToCHB.Contains("1146");
      Console.WriteLine($"input3.ProductsToForceToCHB.Contains(\"1146\") {boolvalue}");

      //      boolvalue = input1.ProductCodes.Intersect(input3.ProductsToForceToCHB).Any();
      Console.WriteLine($" {boolvalue}");

      Console.WriteLine($"Running {nameof(RcgJsonDemo)}....");

      #region Load Workflow using ProductGroupName
      var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "OrderRoutingWorkflows.json", SearchOption.AllDirectories);
      if (files == null || files.Length == 0)
        throw new Exception("Rules not found.");

      var fileData = File.ReadAllText(files[0]);
      var workflow = JsonConvert.DeserializeObject<List<Workflow>>(fileData);
      #endregion

      var reSettingsWithCustomTypes = new ReSettings { CustomTypes = new Type[] { typeof(REUtils) } };
      var bre = new RulesEngine.RulesEngine(workflow.ToArray(), reSettingsWithCustomTypes);

      List<RuleResultTree> resultList;
      string location = "";

      (location, resultList) = NewMethod(input1, input2, bre);

      Console.WriteLine(location);
      PrintExceptions(resultList);

      #region Change to other States
      // change state to NY

      input1.ShipToState = "NY";
      (location, resultList) = NewMethod(input1, input2, bre);

      Console.WriteLine(location);
      PrintExceptions(resultList);

      // change state to MD

      input1.ShipToState = "MD";
      (location, resultList) = NewMethod(input1, input2, bre);

      Console.WriteLine(location);
      PrintExceptions(resultList);

      // change state to LA

      input1.ShipToState = "LA";
      (location, resultList) = NewMethod(input1, input2, bre);

      Console.WriteLine(location);
      PrintExceptions(resultList);

      #endregion

    }

    public record struct RuleExceptionMessage
    {
      public string RuleName { get; set; }
      public string ExceptionMessage { get; set; }
    }

    private static void PrintRule(List<RuleResultTree> resultList)
    {
      List<RuleExceptionMessage> messages = GetExceptionMessages(resultList);

      if (messages.Count > 0)
      {
        string json = JsonConvert.SerializeObject(messages);
        Console.WriteLine(json);
      }
    }
    private static void PrintExceptions(List<RuleResultTree> resultList)
    {
      List<RuleExceptionMessage> messages = GetExceptionMessages(resultList);

      if (messages.Count > 0)
      {
        string json = JsonConvert.SerializeObject(messages);
        Console.WriteLine(json);
      }
    }

    private static List<RuleExceptionMessage> GetExceptionMessages(List<RuleResultTree> resultList)
    {
      List<RuleExceptionMessage> messages = new List<RuleExceptionMessage>();
      foreach (RuleResultTree rrt in resultList)
      {
        if (rrt.ExceptionMessage.StartsWith("Exception", StringComparison.CurrentCultureIgnoreCase))
        {

          RuleExceptionMessage message = new RuleExceptionMessage();
          message.RuleName = rrt.Rule.RuleName;
          message.ExceptionMessage = rrt.ExceptionMessage;
          messages.Add(message);
        }
      }
      return messages;
    }

    private static (string, List<RuleResultTree>) NewMethod(OrderRoutingInput orderRoutingInput, OrderRoutingHelper orderRoutingHelper, RulesEngine.RulesEngine bre)
    {
      List<RuleResultTree> resultList = new List<RuleResultTree>();
      string location = "No Routing was found";

      var rp1 = new RuleParameter("order", orderRoutingInput);
      var rp2 = new RuleParameter("routing", orderRoutingHelper);
      // rp3= orderRoutingDynamic

      resultList = bre.ExecuteAllRulesAsync(orderRoutingInput.ProductGroupName, rp1, rp2).Result;
      resultList.OnSuccess((eventName) => {
        location = $"{orderRoutingInput.ShipToState} Order routed to {eventName}.  {orderRoutingInput.RandomNumber}";
      });

      resultList.OnFail(() => {
        location = $"{orderRoutingInput.ShipToState} Order was not routed";
      });

      return (location, resultList);
    }
  }
}
