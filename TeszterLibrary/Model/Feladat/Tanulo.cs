using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeszterLibrary.ViewModel;

namespace TeszterLibrary.Model.Feladat
{
    public class Tanulo
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Osztaly { get; set; }
        public int Pont { get; set; } = 20; //INFO: Kezdő pontszám, ami a tanulóhoz tartozik
        public DateTime LastModify { get; set; } = DateTime.Now;
        public List<Szorgalmi> Szorgalmik { get; set; } = [];
        public List<FeladatHiany> Hianyok { get; set; } = [];
        public List<HibasFeladat> Hibak { get; set; } = [];
        public List<Pont> Pontok { get; set; } = [];
    }
}
