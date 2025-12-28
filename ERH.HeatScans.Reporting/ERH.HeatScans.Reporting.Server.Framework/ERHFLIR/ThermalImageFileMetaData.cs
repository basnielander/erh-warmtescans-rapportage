using Flir.Atlas.Image;
using System;

namespace ERH.FLIR
{
    public class ThermalImageFileMetaData
    {
        public ThermalImageFileMetaData(ThermalImageFile thermalImageFile)
        {
            PhotoTakenAt = thermalImageFile.DateTaken;
            PalleteName = thermalImageFile.Palette.Name;
        }

        public DateTime PhotoTakenAt { get; private set; }

        /// <summary>
        /// Palette name (should be Rainbow)
        /// </summary>
        public string PalleteName { get; private set; }
    }
}
