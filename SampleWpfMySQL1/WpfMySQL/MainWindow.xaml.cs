using log4net;
using System;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Windows;

using System.IO;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;

using Microsoft.WindowsAPICodePack.Dialogs;

using MySql.Data.MySqlClient;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            // SampleDb.sqlite を作成（存在しなければ）
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                // データベースに接続
                conn.Open();
                // コマンドの実行
                using (var command = conn.CreateCommand())
                {
                    // テーブルが存在しなければ作成する
                    // 種別マスタ
                    StringBuilder sb = new StringBuilder();
                    sb.Append("CREATE TABLE IF NOT EXISTS MSTKIND (");
                    sb.Append("  KIND_CD CHAR(2) NOT NULL");
                    sb.Append("  , KIND_NAME VARCHAR(20)");
                    sb.Append("  , PRIMARY KEY (KIND_CD)");
                    sb.Append(")");
                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    // 猫テーブル
                    sb.Clear();
                    sb.Append("CREATE TABLE IF NOT EXISTS TBLCAT (");
                    sb.Append("  NO INTEGER(2) NOT NULL");
                    sb.Append("  , NAME VARCHAR(20) NOT NULL");
                    sb.Append("  , SEX CHAR(3) NOT NULL");
                    sb.Append("  , AGE INTEGER(1) DEFAULT 0 NOT NULL");
                    sb.Append("  , KIND_CD CHAR(2) DEFAULT '00' NOT NULL");
                    sb.Append("  , FAVORITE VARCHAR(40)");
                    sb.Append("  , PRIMARY KEY (NO)");
                    sb.Append(")");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    // 種別マスタを取得してコンボボックスに設定する
                    using (DataContext con = new DataContext(conn))
                    {
                        // データを取得
                        Table<Kind> mstKind = con.GetTable<Kind>();
                        IQueryable<Kind> result = from x in mstKind orderby x.KindCd select x;

                        // 最初の要素は「指定なし」とする
                        Kind empty = new Kind();
                        empty.KindCd = "";
                        empty.KindName = "指定なし";
                        var list = result.ToList();
                        list.Insert(0, empty);

                        // コンボボックスに設定
                        this.search_kind.ItemsSource = list;
                        this.search_kind.DisplayMemberPath = "KindName";
                    }

                }
                // 切断
                conn.Close();
            }
        }

        /// <summary>
        /// 検索ボタンクリックイベント.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void search_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("検索ボタンクリック");
            searchData();
        }

        /// <summary>
        /// データ検索処理.
        /// </summary>
        private void searchData()
        {
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();

                // 猫データマスタを取得してコンボボックスに設定する
                using (DataContext con = new DataContext(conn))
                {
                    String searchName = this.search_name.Text;
                    String searchKind = (this.search_kind.SelectedValue as Kind).KindCd;

                    // データを取得
                    Table<Cat> tblCat = con.GetTable<Cat>();

                    // サンプルなので適当に組み立てる
                    IQueryable<Cat> result;
                    if (searchKind == "")
                    {
                        // 名前は前方一致のため常に条件していしても問題なし
                        result = from x in tblCat
                                 where x.Name.StartsWith(searchName)
                                 orderby x.No
                                 select x;
                    }
                    else
                    {
                        result = from x in tblCat
                                 where x.Name.StartsWith(searchName) & x.Kind == searchKind
                                 orderby x.No
                                 select x;

                    }
                    this.dataGrid.ItemsSource = result.ToList();

                }

                conn.Close();
            }
        }

        /// <summary>
        /// 追加ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("追加ボタンクリック");

            // 接続
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();

                // データを追加する
                using (DataContext context = new DataContext(conn))
                {
                    // 対象のテーブルオブジェクトを取得
                    var table = context.GetTable<Cat>();
                    // データ作成
                    Cat cat = new Cat();
                    cat.No = 5;
                    cat.Name = "こなつ";
                    cat.Sex = "♀";
                    cat.Age = 7;
                    cat.Kind = "01";
                    cat.Favorite = "布団";
                    // データ追加
                    table.InsertOnSubmit(cat);
                    // DBの変更を確定
                    context.SubmitChanges();
                }
                conn.Close();
            }
            // データ再検索
            searchData();
            MessageBox.Show("データを追加しました。");
        }

        /// <summary>
        /// 更新ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void upd_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("更新ボタンクリック");

            // 選択チェック
            if (this.dataGrid.SelectedItem == null)
            {
                MessageBox.Show("更新対象を選択してください。");
                return;
            }

            // 接続
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();

                // データを追加する
                using (DataContext context = new DataContext(conn))
                {
                    // 対象のテーブルオブジェクトを取得
                    var table = context.GetTable<Cat>();
                    // 選択されているデータを取得
                    Cat cat = this.dataGrid.SelectedItem as Cat;
                    // テーブルから対象のデータを取得
                    var target = table.Single(x => x.No == cat.No);
                    // データ変更
                    target.Favorite = "高いところ";
                    // DBの変更を確定
                    context.SubmitChanges();
                }
                conn.Close();
            }

            // データ再検索
            searchData();

            MessageBox.Show("データを更新しました。");
        }

        /// <summary>
        /// 削除ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void del_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("追加ボタンクリック");

            // 選択チェック
            if (this.dataGrid.SelectedItem == null)
            {
                MessageBox.Show("削除対象を選択してください。");
                return;
            }

            // 接続
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();

                // データを削除する
                using (DataContext context = new DataContext(conn))
                {
                    // 対象のテーブルオブジェクトを取得
                    var table = context.GetTable<Cat>();
                    // 選択されているデータを取得
                    Cat cat = this.dataGrid.SelectedItem as Cat;
                    // テーブルから対象のデータを取得
                    var target = table.Single(x => x.No == cat.No);
                    // データ削除
                    table.DeleteOnSubmit(target);
                    // DBの変更を確定
                    context.SubmitChanges();
                }
                conn.Close();
            }

            // データ再検索
            searchData();

            MessageBox.Show("データを削除しました。");
        }

        /// <summary>
        /// CSV読込ボタンクリックイベント.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imp_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.csv";
            if (ofd.ShowDialog() == false)
            {
                return;
            }

            List<Cat> list = readFile(ofd.FileName);

            // 接続
            int count = 0;
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            {
                conn.Open();

                // データを追加する
                using (DataContext context = new DataContext(conn))
                {
                    foreach (Cat cat in list)
                    {
                        // 対象のテーブルオブジェクトを取得
                        var table = context.GetTable<Cat>();
                        // データが存在するかどうか判定
                        if (table.SingleOrDefault(x => x.No == cat.No) == null)
                        {
                            // データ追加
                            table.InsertOnSubmit(cat);
                            // DBの変更を確定
                            context.SubmitChanges();
                            count++;
                        }
                    }
                }
                conn.Close();
            }

            MessageBox.Show(count + " / " + list.Count + " 件 のデータを取り込みました。");

            // データ再検索
            searchData();
        }

        /// <summary>
        /// CSVファイル読み込み処理
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static List<Cat> readFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string ret = string.Empty;
            List<Cat> list = new List<Cat>();
            using (TextFieldParser tfp = new TextFieldParser(fileInfo.FullName, Encoding.GetEncoding("Shift_JIS")))
            {
                tfp.TextFieldType = FieldType.Delimited;
                tfp.Delimiters = new string[] { "," };
                tfp.HasFieldsEnclosedInQuotes = true;
                tfp.TrimWhiteSpace = true;
                while (!tfp.EndOfData)
                {
                    string[] fields = tfp.ReadFields();
                    Cat cat = new Cat();
                    cat.No = int.Parse(fields[0]);
                    cat.Name = fields[1];
                    cat.Sex = fields[2];
                    cat.Age = int.Parse(fields[3]);
                    cat.Kind = fields[4];
                    cat.Favorite = fields[5];
                    list.Add(cat);
                }
            }
            return list;
        }

        /// <summary>
        /// CSV出力ボタンクリックイベント.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exp_button_Click(object sender, RoutedEventArgs e)
        {
            // ファイル保存ダイアログ
            SaveFileDialog dlg = new SaveFileDialog();

            // デフォルトファイル名
            dlg.FileName = "cat.csv";

            // デフォルトディレクトリ
            dlg.InitialDirectory = @"c:\";

            // ファイルのフィルタ
            dlg.Filter = "CSVファイル|*.csv|すべてのファイル|*.*";

            // ファイルの種類
            dlg.FilterIndex = 0;

            // 指定されたファイル名を取得

            if (dlg.ShowDialog() == true)
            {
                List<Cat> list = this.dataGrid.ItemsSource as List<Cat>;
                String delmiter = ",";
                StringBuilder sb = new StringBuilder();
                Cat lastData = list.Last();
                foreach (Cat cat in list)
                {
                    sb.Append(cat.No).Append(delmiter);
                    sb.Append(cat.Name).Append(delmiter);
                    sb.Append(cat.Sex).Append(delmiter);
                    sb.Append(cat.Age).Append(delmiter);
                    sb.Append(cat.Kind).Append(delmiter);
                    sb.Append(cat.Favorite);
                    if (!cat.Equals(lastData))
                    {
                        sb.Append(Environment.NewLine);
                    }
                }

                Stream st = dlg.OpenFile();
                StreamWriter sw = new StreamWriter(st, Encoding.GetEncoding("UTF-8"));

                sw.Write(sb.ToString());
                sw.Close();
                st.Close();
                MessageBox.Show("CSVファイルを出力しました。");
            }
            else
            {
                MessageBox.Show("キャンセルされました。");
            }

        }

        /// <summary>
        /// ディレクトリ内のCSVファイルを全て読み込む
        /// </summary>
        /// <param name="sourceDir"></param>
        private List<Cat> readFiles(String sourceDir)
        {
            string[] files = Directory.GetFiles(sourceDir, "*.csv");
            List<Cat> list = new List<Cat>();
            // リストを走査してコピー
            for (int fileCount = 0; fileCount < files.Length; fileCount++)
            {
                List<Cat> catList = readFile(files[fileCount]) as List<Cat>;
                list.AddRange(catList);
            }

            return list;
        }

        /// <summary>
        /// フォルダ参照ボタンクリックイベント.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fld_button_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログ生成
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();

            // パラメタ設定

            // タイトル
            dlg.Title = "フォルダ選択";
            // フォルダ選択かどうか
            dlg.IsFolderPicker = true;
            // 初期ディレクトリ
            dlg.InitialDirectory = @"c:\";
            // ファイルが存在するか確認する
            //dlg.EnsureFileExists = false;
            // パスが存在するか確認する
            //dlg.EnsurePathExists = false;
            // 読み取り専用フォルダは指定させない
            //dlg.EnsureReadOnly = false;
            // コンパネは指定させない
            //dlg.AllowNonFileSystemItems = false;

            //ダイアログ表示
            var Path = dlg.ShowDialog();
            if (Path == CommonFileDialogResult.Ok)
            {
                // 選択されたフォルダ名を取得、格納されているCSVを読み込む
                List<Cat> list = readFiles(dlg.FileName);

                // 接続
                int count = 0;
                //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
                using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
                {
                    conn.Open();

                    // データを追加する
                    using (DataContext context = new DataContext(conn))
                    {
                        foreach (Cat cat in list)
                        {
                            // 対象のテーブルオブジェクトを取得
                            var table = context.GetTable<Cat>();
                            // データが存在するかどうか判定
                            if (table.SingleOrDefault(x => x.No == cat.No) == null)
                            {
                                // データ追加
                                table.InsertOnSubmit(cat);
                                // DBの変更を確定
                                context.SubmitChanges();
                                count++;
                            }
                        }
                    }
                    conn.Close();
                }

                MessageBox.Show(count + " / " + list.Count + " 件 のデータを取り込みました。");

                // データ再検索
                searchData();
            }
        }
    }
}