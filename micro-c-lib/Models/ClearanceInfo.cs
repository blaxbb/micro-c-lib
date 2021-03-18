using MicroCLib.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace micro_c_lib.Models
{
    public class ClearanceInfo : NotifyPropertyChangedItem
    {
        private string state;
        private float price;

        public string State { get => state; set => SetProperty(ref state, value); }
        public float Price { get => price; set => SetProperty(ref price, value); }
    }
}
