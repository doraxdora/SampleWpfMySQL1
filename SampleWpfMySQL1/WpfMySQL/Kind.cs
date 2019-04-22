using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq.Mapping;

namespace WpfApp1
{
    [Table(Name = "mstkind")]
    public class Kind
    {
        [Column(Name = "kind_cd", IsPrimaryKey = true)]
        public String KindCd { get; set; }
        [Column(Name = "kind_name")]
        public String KindName { get; set; }
    }
}
