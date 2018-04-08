using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Sensor.Model
{
    public class Client
    {
        public int Name { get; set; }
        public int Value { get; set; }
        public TextBlock UIText;
    }
}