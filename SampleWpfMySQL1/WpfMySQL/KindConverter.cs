using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq;

// Converter 用
// IValueConverter、CultureInfo
using System.Windows.Data;
using System.Globalization;

using MySql.Data.MySqlClient;

namespace WpfApp1
{
    /// <summary>
    /// 種別コンバータークラス.
    /// </summary>
    public class KindConverter : IValueConverter
    {
        /// <summary>
        /// データ変換処理
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 都度DBアクセスするのはどうかと思うが
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();
                using (DataContext con = new DataContext(conn))
                {
                    // データを取得
                    Table<Kind> tblCat = con.GetTable<Kind>();
                    Kind k = tblCat.Single(c => c.KindCd == value as String);
                    if (k != null)
                    {
                        return k.KindName;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// データ復元処理
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();
                using (DataContext con = new DataContext(conn))
                {
                    // データを取得
                    Table<Kind> tblCat = con.GetTable<Kind>();
                    Kind k = tblCat.Single(c => c.KindName == value as String);
                    if (k != null)
                    {
                        return k.KindCd;
                    }

                }
            }
            return "";
        }
    }
}