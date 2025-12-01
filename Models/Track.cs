using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AudioPlayer.Models
{
    public class Track
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }

        public byte[]? CoverData { get; set; }

    }
}
