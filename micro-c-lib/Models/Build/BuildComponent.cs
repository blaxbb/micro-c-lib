﻿using MicroCLib.Models.Reference;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MicroCLib.Models.Reference.PlanReference;

namespace MicroCLib.Models
{
    public class BuildComponent : NotifyPropertyChangedItem
    {
        private Item? item;
        public Item? Item { get => item; set { SetProperty(ref item, value); OnPropertyChanged(nameof(ComponentLabel)); } }

        public enum ComponentType
        {
            BuildService,
            CPU,
            Motherboard,
            RAM,
            Case,
            PowerSupply,
            GPU,
            SSD,
            HDD,
            CPUCooler,
            CaseFan,
            OperatingSystem,
            Miscellaneous,
            Plan,
            WaterCoolingKit,
            Desktop,
            Laptop,
            Monitor,
            Printer,
            Keyboard,
            Mouse,
            Television,
            CellPhone,
            DigitalCamera,
            Webcam,
            FlashMemory,
            HomeAutomation,
            SecurityCamera,
            SecurityCameraKit,
            WirelessRouter,
            WiredRouter,
            WiredNetworkAdapter,
            NetworkingPowerline,
            POENetworkAdapter,
            NetworkSwitch,
            WirelessAdapter,
            WirelessAccessPoint,
            WirelessBoosters,
            BluetoothAdapter,
            NetworkingBridge,
            NetworkingCable,
            NetworkingAccessory,
            NetworkAttachedStorage,
            UninteruptablePowerSupply,
            InkAndToner,
            Printer3D,
            Headphones,
            Speakers,
            GameControllers,
            GameAccessories,
            VirutalReality,
            Xbox,
            Playstation,
            Nintendo,
            ExternalDrives,
            HomeTheaterAudio,
            HomeTheaterWireless,
            Projectors,
            StreamingMedia,
            None,
        }
        public ComponentType Type { get; set; } = ComponentType.None;
        [JsonIgnore]
        public string CategoryFilter => CategoryFilterForType(Type);
        [JsonIgnore]
        public string ComponentLabel => Item == null ? Type.ToString() : $"{Item.Name}";
        [JsonIgnore]
        //public string ErrorText => String.Join("\n", Dependencies.Where(d => !d.Compatible()).Select(d => d.ErrorText));
        private string errorText;
        private string hintText;
        private ObservableCollection<string> serials;

        [JsonIgnore]
        public string ErrorText { get => errorText; set => SetProperty(ref errorText, value); }
        [JsonIgnore]
        public string HintText { get => hintText; set => SetProperty(ref hintText, value); }

        public ObservableCollection<string> Serials { get => serials; set => SetProperty(ref serials, value); }

        public BuildComponent()
        {
            Serials = new ObservableCollection<string>();
        }

        public bool IsPlanApplicable()
        {
            switch (Type)
            {
                case ComponentType.OperatingSystem:
                case ComponentType.Plan:
                case ComponentType.CellPhone:
                case ComponentType.UninteruptablePowerSupply:
                case ComponentType.InkAndToner:
                    return false;
                default:
                    return true;
            }

        }

        public bool AutoSearch()
        {
            switch (Type)
            {
                case ComponentType.OperatingSystem:
                case ComponentType.BuildService:
                    return true;
                default:
                    return false;
            }

        }

        public static string CategoryFilterForType(ComponentType type)
        {
            //from microcenter.com search N field ex: &N=4294966995

            return type switch
            {
                ComponentType.CPU => "4294966995",
                ComponentType.Motherboard => "4294966996",
                ComponentType.RAM => "4294966965",
                ComponentType.Case => "4294964318",
                ComponentType.PowerSupply => "4294966654",
                ComponentType.GPU => "4294966937",
                ComponentType.SSD => "4294945779",
                ComponentType.HDD => "4294945772",
                ComponentType.CPUCooler => "4294966927",
                ComponentType.CaseFan => "4294966926",
                ComponentType.OperatingSystem => "4294967276",
                ComponentType.BuildService => "4294812275",
                ComponentType.WaterCoolingKit => "4294966904",
                ComponentType.Desktop => "4294967292",
                ComponentType.Laptop => "4294967288",
                ComponentType.Monitor => "4294966896",
                ComponentType.Printer => "4294966830",
                ComponentType.Keyboard => "4294966800",
                ComponentType.Mouse => "4294966799",
                ComponentType.Television => "4294966839",
                ComponentType.CellPhone => "4294939905",
                ComponentType.DigitalCamera => "4294966817",
                ComponentType.Webcam => "4294966765",
                ComponentType.FlashMemory => "4294966791",
                ComponentType.HomeAutomation => "4294966669",
                ComponentType.SecurityCamera => "4294966674",
                ComponentType.SecurityCameraKit => "4294966672",
                ComponentType.WirelessRouter => "4294966869",
                ComponentType.WiredRouter => "4294966883",
                ComponentType.WiredNetworkAdapter => "4294966885",
                ComponentType.NetworkingPowerline => "4294966880",
                ComponentType.POENetworkAdapter => "4294966879",
                ComponentType.NetworkSwitch => "4294966884",
                ComponentType.WirelessAdapter => "4294966871",
                ComponentType.WirelessAccessPoint => "4294966873",
                ComponentType.WirelessBoosters => "4294966876",
                ComponentType.BluetoothAdapter => "4294966875",
                ComponentType.NetworkingBridge => "4294966874",
                ComponentType.NetworkingCable => "4294966745",
                ComponentType.NetworkingAccessory => "4294966692",
                ComponentType.NetworkAttachedStorage => "4294945770",
                ComponentType.UninteruptablePowerSupply => "4294966640",
                ComponentType.InkAndToner => "4294961538",
                ComponentType.Printer3D => "4294828445",
                ComponentType.Headphones => "4294966661",
                ComponentType.Speakers => "4294966662",
                ComponentType.GameControllers => "4294961019",
                ComponentType.GameAccessories => "4294964277",
                ComponentType.VirutalReality => "4294854801",
                ComponentType.Xbox => "4294854828",
                ComponentType.Playstation => "4294854864",
                ComponentType.Nintendo => "4294967264",
                ComponentType.ExternalDrives => "4294945771",
                ComponentType.HomeTheaterAudio => "4294964309",
                ComponentType.HomeTheaterWireless => "4294964306",
                ComponentType.Projectors => "4294943185",
                ComponentType.StreamingMedia => "4294936345",
                _ => "",
            };
        }

        public static ComponentType TypeForCategoryFilter(string filter)
        {
            foreach (ComponentType val in Enum.GetValues(typeof(ComponentType)))
            {
                var check = CategoryFilterForType(val);
                if (check == filter)
                {
                    return val;
                }
            }

            return ComponentType.None;
        }

        public static IEnumerable<PlanType> ApplicablePlans(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Laptop:
                    yield return PlanType.Laptop_DOP;
                    yield return PlanType.Laptop_ADH;
                    yield return PlanType.Laptop_Extension;
                    break;
                case ComponentType.Desktop:
                    yield return PlanType.Desktop_DOP;
                    yield return PlanType.Desktop_ADH;
                    yield return PlanType.Desktop_Extension;
                    break;
                case ComponentType.BuildService:
                    yield return PlanType.Build_Plan;
                    break;
                default:
                    yield return PlanType.Replacement;
                    yield return PlanType.Carry_In;
                    break;
            }
        }

        public IEnumerable<PlanReference> ApplicablePlans()
        {
            if (!IsPlanApplicable())
            {
                yield break;
            }

            if(Item == null)
            {
                yield break;
            }

            var types = ApplicablePlans(Type);
            foreach(var type in types)
            {
                var plan = PlanReference.Get(type, Item.Price);
                if (plan != null)
                {
                    yield return plan;
                }
            }
        }

        public static int MaxNumberPerType(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.Plan:
                    return 0;
                case ComponentType.CPU:
                case ComponentType.Motherboard:
                case ComponentType.Case:
                case ComponentType.PowerSupply:
                case ComponentType.CPUCooler:
                case ComponentType.OperatingSystem:
                case ComponentType.BuildService:
                    return 1;
                case ComponentType.GPU:
                case ComponentType.WaterCoolingKit:
                    return 4;
                case ComponentType.RAM:
                case ComponentType.CaseFan:
                    return 8;
                case ComponentType.SSD:
                    return 64;
                case ComponentType.HDD:
                case ComponentType.Miscellaneous:
                default:
                    return 64;
            }
        }

        public static string MCOLSelectorIDForType(ComponentType type)
        {
            switch (type)
            {
                case ComponentType.CPU:
                    return "1";
                case ComponentType.Motherboard:
                    return "2";
                case ComponentType.RAM:
                    return "7";
                case ComponentType.Case:
                    return "5";
                case ComponentType.PowerSupply:
                    return "6";
                case ComponentType.GPU:
                    return "8";
                case ComponentType.SSD:
                    return "15";
                case ComponentType.HDD:
                    return "14";
                case ComponentType.CPUCooler:
                    return "9";
                case ComponentType.CaseFan:
                    return "90";
                case ComponentType.BuildService:
                case ComponentType.Plan:
                case ComponentType.OperatingSystem:
                    return "12";
                case ComponentType.WaterCoolingKit:
                    return "84";
                case ComponentType.Miscellaneous:
                default:
                    return "19";
            }
        }

        public static async Task<string> ExportToMCOL(List<BuildComponent> Components, IProgress<int> progress)
        {
            string BuildURL = "";
            using (var client = new HttpClient())
            {
                List<string> hitCategories = new List<string>();
                int cnt = 0;
                for (int i = 0; i < Components.Count; i++)
                {
                    var comp = Components[i];
                    if (comp.Item == null || comp.Item.ID == null)
                    {
                        continue;
                    }

                    cnt++;
                    progress.Report(cnt);


                    var selectorID = BuildComponent.MCOLSelectorIDForType(comp.Type);
                    bool duplicateSelector = hitCategories.Contains(selectorID);
                    var url = $"https://www.microcenter.com/site/content/custom-pc-builder.aspx?toselectorId={selectorID}&configuratorId=1&productId={comp.Item.ID}&productName={comp.Item.Name}&newItem={(duplicateSelector ? "true" : "false")}";
                    if (!duplicateSelector)
                    {
                        hitCategories.Add(selectorID);
                    }

                    var result = await client.GetAsync(url);
                    var body = await result.Content.ReadAsStringAsync();

                    //Get url
                    var match = Regex.Match(body, "value=\"(.*?)\" name=\"shareURL\" id=\"shareURL\">");
                    if (match.Success)
                    {
                        BuildURL = match.Groups[1].Value;
                    }

                    //get section with component id
                    match = Regex.Match(body, "id=\"selector_1\"(.*?)newItem", RegexOptions.Singleline);
                    if (match.Success)
                    {
                        var componentText = match.Groups[1].Value;
                        //collect all of the items in that section
                        var matches = Regex.Matches(componentText, "id=\"selector_(.*?)\"");
                        if (matches.Count > 0)
                        {
                            var selectors = matches.OfType<Match>().Select(m => m.Groups[1]);

                        }
                    }
                }
            }

            return BuildURL;
        }
    }
}
