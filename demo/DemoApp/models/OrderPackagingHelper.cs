using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace OrderPackagingWorkflow
{
  public record struct ProductGroupLocationVolume
  {
    public string LocationName { get; set; }
    public int ProductGroupItemsInProcess { get; set; }
    public int ProductGroupOrdersInProcess { get; set; }
    public int ProductCategoryOrdersInProcess { get; set; }
    public int ProductCategoryItemsInProcess { get; set; }

    public int ProductCategoryItemsPerDay { get; set; }
  }
  public record struct ProductGroupVolume
  {
    public int ProductGroupId { get; set; }
    public List<ProductGroupLocationVolume> LocationVolumes { get; set; }
  }

  public record class OrderRoutingHelper
  {
    public List<ProductGroupVolume> ProductGroupVolumes { get; set; }

    public readonly List<string> LocationNames = new List<string> { "BVL", "CHB", "LVL", "PHX", "UK" };

    public List<string> NewEnglandStates { get; set; }
    public List<string> NorthAtlanticStates { get; set; }
    public List<string> SouthAtlanticStates { get; set; }
    public List<string> WestNorthCentralStates { get; set; }
    public List<string> EastNorthCentralStates { get; set; }
    public List<string> WestSouthCentralStates { get; set; }
    public List<string> EastSouthCentralStates { get; set; }
    public List<string> PacificStates { get; set; }
    public List<string> MountainStates { get; set; }
    public List<string> USTerritories { get; set; }
    public List<string> EUCountries { get; set; }
    public List<string> PHXStates { get; set; }
    public List<string> ActiveProductsBVL { get; set; }
    public List<string> ActiveProductsCHB { get; set; }
    public List<string> ActiveProductsLVL { get; set; }
    public List<string> ActiveProductsPHX { get; set; }
    public List<string> ActiveProductsUK { get; set; }
    public static string ToJson(OrderRoutingHelper orderRoutingHelper)
    {
      string output = JsonConvert.SerializeObject(orderRoutingHelper);
      return output;
    }
  }
}
