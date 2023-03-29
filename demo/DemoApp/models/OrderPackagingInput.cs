using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace OrderPackagingWorkflow
{
  public class OrderPackagingInput
  {
    public string ProductGroupId { get; set; }
    public string ProductGroupName { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; }
    public string PartnerId { get; set; }
    public string PartnerName { get; set; }
    public string PSGMasterId { get; set; }
    public string PSGName { get; set; }
    public string ShipMethod { get; set; }
    public string ShipToCountryCode { get; set; }
    public string ShipToState { get; set; }
    public DateTime DateCreated { get; set; }
    public string RandomNumber { get; set; }
    public string PriorityCode { get; set; }
    public List<string> ProductCodes { get; set; }

    public float TotalQty { get; set; }
    public float Qty4InchPrints { get; set; }
    public float Qty5InchPrints { get; set; }
    public float Qty8InchPrints { get; set; }
    public bool IsWeekDay() =>
      DateCreated.DayOfWeek switch {
        DayOfWeek.Saturday => false,
        DayOfWeek.Sunday => false,
        _ => true
      };

    public static string ToJson(OrderPackagingInput orderPackagingInput)
    {
      string output = JsonConvert.SerializeObject(orderPackagingInput, Formatting.Indented);
      return output;
    }
  }

}

