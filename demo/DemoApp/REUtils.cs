using System;
using System.Linq;
using System.Collections.Generic;

namespace RE.HelperFunctions
{
  public static class REUtils
  {
    public static bool StringInCSVList(string check, string csvList)
    {
      if (String.IsNullOrEmpty(check) || String.IsNullOrEmpty(csvList))
        return false;

      var list = csvList.Split(',').ToList();
      return list.Contains(check);
    }
    public static bool IntegerInCSVList(int check, string csvList)
    {
      if (String.IsNullOrEmpty(csvList))
        return false;

      var list = csvList.Split(',').ToList();
      return list.Contains(check.ToString());
    }
    public static bool ListsIntersectAnyValue(List<string> list1, List<string> list2)
    {
      if (list1.Count == 0 || list2.Count == 0) return false;

      var result = list1.Intersect(list2).Any();
      return result;
    }
    public static bool ListsIntersectAnyValue(List<string> list1, string csvList)
    {
      if (list1.Count < 1 || String.IsNullOrEmpty(csvList) == true) return false;

      List<string> list2 = csvList.Split(',').ToList<string>();
      return list1.Intersect(list2).Any();
    }
    public static bool AllList2ValuesIntersect(List<string> list1, string csvString)
    {
      if (list1.Count < 1 || String.IsNullOrEmpty(csvString) == true) return false;

      List<string> list2 = csvString.Split(',').ToList();
      return (list2.Count() == list1.Intersect(list2).Count());
    }
    public static bool AllList2ValuesIntersect(List<string> list1, List<string> list2)
    {
      if (list1.Count == 0 || list2.Count == 0) return false;

      var result = list2.Count() == list1.Intersect(list2).Count();

      return result;
    }
  }
}