using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotMGAssetExtractor.Model
{
    public class EquipmentSet
    {
        public int type { get; set; }
        public string id { get; set; }
        public string Setpiece { get; set; }
        public string ActivateOnEquipAll { get; set; }
        public string ActivateOnEquip3 { get; set; }
        public string ActivateOnEquip2 { get; set; }
        public string ActivateOnEquipCustom { get; set; }

    }
}
