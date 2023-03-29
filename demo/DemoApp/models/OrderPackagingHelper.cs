using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace OrderPackagingWorkflow
{
  
  public record class OrderPackagingHelper
  {
 
    public readonly List<string> LocationNames = new List<string> { "BVL", "CHB", "LVL", "PHX", "UK" };

    public List<string> PrintGiftBoxCodes { get; set; }
    public List<int> IgnoreSmallWalletBrands { get; set; }
    public static string ToJson(OrderPackagingHelper orderPackagingHelper)
    {
      string output = JsonConvert.SerializeObject(orderPackagingHelper);
      return output;
    }
  }
}
