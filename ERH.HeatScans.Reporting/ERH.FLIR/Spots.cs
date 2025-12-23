using Flir.Atlas.Image;
using System.Collections.Generic;
using System.Linq;

namespace ERH.FLIR
{
    public class Spots : List<Spot>
    {
        public Spots(ThermalImageFile thermalImageFile) : base(thermalImageFile.Measurements.Count)
        {
            AddRange(thermalImageFile.Measurements.Select(measurement => new Spot(measurement)));
        }
    }
}
