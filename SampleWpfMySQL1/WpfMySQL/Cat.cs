using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq.Mapping;

namespace WpfApp1
{
    [Table(Name = "tblcat")]
    public class Cat
    {

        [Column(Name = "no", IsPrimaryKey = true)]
        public int No { get; set; }
        [Column(Name = "name")]
        public String Name { get; set; }
        [Column(Name = "sex")]
        public String Sex { get; set; }
        [Column(Name = "age")]
        public int Age { get; set; }
        [Column(Name = "kind_cd")]
        public String Kind { get; set; }
        [Column(Name = "favorite")]
        public String Favorite { get; set; }
    }
}
