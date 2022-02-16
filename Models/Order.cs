using System.ComponentModel;

namespace OTS_TEST.Models
{
    public class Order
    {
        public long Id { get; set; }
        [DisplayName("Код")]
        public string Code { get; set; }
        [DisplayName("Наименование")]
        public string Name { get; set; }
        [DisplayName("Электронная закупка")]
        public bool IsElectronic { get; set; }
        [DisplayName("Признак конкурентного способа закупки")]
        public bool Competitive { get; set; }
        [DisplayName("Полное имя")]
        public string FullName { get; set; }
        [DisplayName("ИНН")]
        public string INN { get; set; }
        [DisplayName("КПП")]
        public string KPP { get; set; }
    }
}
