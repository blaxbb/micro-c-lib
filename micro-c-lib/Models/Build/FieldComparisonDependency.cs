﻿using MicroCLib.Models.Build;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MicroCLib.Models
{
    public class FieldComparisonDependency : BuildComponentDependency, IBuildComponentDependency
    {
        public BuildComponent.ComponentType FirstType { get; set; }
        public BuildComponent.ComponentType SecondType { get; set; }

        public string FirstFieldName { get; set; } = "";
        public string SecondFieldName { get; set; } = "";

        public CompareMode Mode { get; set; }
        public bool FailOnEmpty { get; set; }


        public enum CompareMode
        {
            LessThan,
            LessThanOrEqual,
            Equal,
            GreaterThanOrEqual,
            GreaterThan
        }
        public FieldComparisonDependency(string name, BuildComponent.ComponentType first, string fieldFirst, BuildComponent.ComponentType second, string fieldSecond, CompareMode mode)
            : base(name)
        {
            FirstType = first;
            SecondType = second;
            FirstFieldName = fieldFirst;
            SecondFieldName = fieldSecond;
            Mode = mode;
        }

        //public override string HintText()
        //{
        //    return $"{FirstFieldName} {Mode} {SecondFieldName}";
        //}

        public override string HintText(List<Item> items, BuildComponent.ComponentType type)
        {
            if(type != FirstType && type != SecondType)
            {
                return null;
            }

            var secondaryItems = items.Where(i => i.ComponentType == SecondType);
            var primaryItems = items.Where(i => i.ComponentType == FirstType);
            var primarySum = primaryItems.Sum(i => GetValue(i, FirstFieldName));
            var secondarySum = secondaryItems.Sum(i => GetValue(i, SecondFieldName));

            if(type == FirstType)
            {
                if(secondaryItems.Count() == 0)
                {
                    return null;
                }

                return $"Must have {FirstFieldName} {GetModeString(Mode)} {secondarySum} ({SecondType}) (Currently {primarySum})";
            }
            if(type == SecondType)
            {
                if (primaryItems.Count() == 0)
                {
                    return null;
                }
                return $"Must have {SecondFieldName} {GetModeString(Reverse(Mode))} {primarySum} ({FirstType}) (Currently {secondarySum})";
            }

            return null;
        }

        public static bool Compare(float f1, float f2, CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.LessThan:
                    return f1 < f2;
                case CompareMode.LessThanOrEqual:
                    return f1 <= f2;
                case CompareMode.Equal:
                    return f1 == f2;
                case CompareMode.GreaterThanOrEqual:
                    return f1 >= f2;
                case CompareMode.GreaterThan:
                    return f1 > f2;
                default:
                    return false;
            }
        }

        private static string GetModeString(CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.LessThan:
                    return "≺";
                case CompareMode.LessThanOrEqual:
                    return "≤";
                case CompareMode.Equal:
                    return "=";
                case CompareMode.GreaterThanOrEqual:
                    return "≥";
                case CompareMode.GreaterThan:
                    return "≻";
                default:
                    return "?";
            }
        }

        private static CompareMode Reverse(CompareMode mode)
        {
            switch (mode)
            {
                case CompareMode.LessThan:
                    return CompareMode.GreaterThan;
                case CompareMode.LessThanOrEqual:
                    return CompareMode.GreaterThanOrEqual;
                case CompareMode.Equal:
                    return CompareMode.Equal;
                case CompareMode.GreaterThanOrEqual:
                    return CompareMode.LessThanOrEqual;
                case CompareMode.GreaterThan:
                    return CompareMode.LessThan;
                default:
                    return mode;
            }
        }

        public override List<DependencyResult> HasErrors(List<Item> items)
        {
            var primaryItems = items.Where(i => i.ComponentType == FirstType);
            var secondaryItems = items.Where(i => i.ComponentType == SecondType);

            if (FailOnEmpty)
            {
                if (primaryItems.Count(i => i.Specs.ContainsKey(FirstFieldName)) == 0)
                {
                    return primaryItems.Select(i => new DependencyResult(i, $"Cannot determine value of {FirstFieldName}"))
                            .Concat(
                                secondaryItems.Select(i => new DependencyResult(i, $"Cannot determine value of {FirstFieldName} on {FirstType}"))
                            ).ToList();
                }

                if (secondaryItems.Count(i => i.Specs.ContainsKey(SecondFieldName)) == 0)
                {
                    return secondaryItems.Select(i => new DependencyResult(i, $"Cannot determine value of {SecondFieldName}"))
                            .Concat(
                                primaryItems.Select(i => new DependencyResult(i, $"Cannot determine value of {SecondFieldName} on {SecondType}"))
                            ).ToList();
                }
            }
            else
            {
                if (primaryItems.Count(i => i.Specs.ContainsKey(FirstFieldName)) == 0)
                {
                    return new List<DependencyResult>();
                }

                if (secondaryItems.Count(i => i.Specs.ContainsKey(SecondFieldName)) == 0)
                {
                    return new List<DependencyResult>();
                }
            }


            var primarySum = primaryItems.Sum(i => GetValue(i, FirstFieldName));
            var secondarySum = secondaryItems.Sum(i => GetValue(i, SecondFieldName));

            if (!Compare(primarySum, secondarySum, Mode))
            {
                return secondaryItems.Concat(primaryItems)
                    .Select(i =>
                        new DependencyResult(i, $"{FirstFieldName} ({primarySum}) {GetModeString(Mode)} {SecondFieldName} ({secondarySum})")
                    ).ToList();
            }


            return new List<DependencyResult>();
        }

        private static float GetValue(Item item, string field)
        {
            if (item == null || item.Specs == null || !item.Specs.ContainsKey(field))
            {
                return 0;
            }

            var spec = item.Specs[field];
            var specNumber = Regex.Match(spec, "([\\d\\.]+)").Groups[1].Value;
            if (float.TryParse(specNumber, out float val))
            {
                return val;
            }

            return 0;
        }
    }
}