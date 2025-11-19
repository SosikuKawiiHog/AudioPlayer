using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlayer.Models
{
    public class Track
    {
        public string Path { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }

        //реализовать чтение обложки или её генерацию по метаданным эээээээ типа
        public string CoverPath { get; set; } = "swag.png";
    }
}
