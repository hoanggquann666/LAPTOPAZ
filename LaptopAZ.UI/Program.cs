using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LaptopAZ.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Tự động kiểm tra và khởi tạo cơ sở dữ liệu cục bộ
            InitializeDatabase();

            Application.Run(new LoginForm());
        }

        private static bool InitializeDatabase()
        {
            try
            {
                var connStringSettings = ConfigurationManager.ConnectionStrings["LaptopAZDbContext"];
                if (connStringSettings == null) return true; // Để EF tự xử lý hoặc ném ngoại lệ tự nhiên

                string connString = connStringSettings.ConnectionString;
                
                // Phân tích máy chủ và tên database từ connection string
                var builder = new SqlConnectionStringBuilder(connString);
                string dbName = builder.InitialCatalog;
                
                // 1. Kiểm tra xem DB đã tồn tại chưa bằng cách kết nối tới 'master'
                builder.InitialCatalog = "master";
                string masterConnString = builder.ConnectionString;

                bool dbExists = false;
                using (var conn = new SqlConnection(masterConnString))
                {
                    conn.Open();
                    string checkQuery = $"SELECT database_id FROM sys.databases WHERE name = '{dbName}'";
                    using (var cmd = new SqlCommand(checkQuery, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        dbExists = (result != null && result != DBNull.Value);
                    }
                }

                if (dbExists)
                {
                    return true; // Database đã tồn tại, hoạt động bình thường!
                }

                // 2. Database chưa tồn tại! Tiến hành tạo mới
                using (var conn = new SqlConnection(masterConnString))
                {
                    conn.Open();
                    string createDbQuery = $"CREATE DATABASE [{dbName}]";
                    using (var cmd = new SqlCommand(createDbQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Chờ 1 giây để SQL Server khởi tạo xong database vật lý
                System.Threading.Thread.Sleep(1000);

                // 3. Tìm và thực thi tệp tin SQL script khởi tạo
                string sqlFilePath = FindSqlScriptPath();
                if (string.IsNullOrEmpty(sqlFilePath))
                {
                    MessageBox.Show(
                        "Hệ thống phát hiện Cơ sở dữ liệu 'LaptopAZDB' chưa được khởi tạo cục bộ và đã tự động tạo database mới.\n\n" +
                        "Tuy nhiên, KHÔNG tìm thấy tệp tin SQL khởi tạo 'LaptopAZDatabase.sql' để nạp cấu trúc và dữ liệu mẫu.\n" +
                        "Ứng dụng vẫn sẽ khởi chạy, nhưng có thể gặp lỗi kết nối bảng dữ liệu.",
                        "Cảnh báo khởi tạo CSDL",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                string scriptText = File.ReadAllText(sqlFilePath);
                
                // Tách toàn bộ câu lệnh SQL bằng từ khóa GO trên một dòng riêng biệt
                var commandTexts = Regex.Split(
                    scriptText,
                    @"^\s*GO\s*$",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase
                );

                // Thực thi tuần tự từng block lệnh SQL trên connection của database LaptopAZDB
                builder.InitialCatalog = dbName;
                string dbConnString = builder.ConnectionString;

                using (var conn = new SqlConnection(dbConnString))
                {
                    conn.Open();
                    foreach (var cmdText in commandTexts)
                    {
                        if (string.IsNullOrWhiteSpace(cmdText)) continue;
                        
                        // Bỏ qua câu lệnh USE LaptopAZDB; vì ta đã kết nối trực tiếp vào database này rồi
                        if (cmdText.Trim().StartsWith("USE ", StringComparison.OrdinalIgnoreCase)) continue;

                        using (var cmd = new SqlCommand(cmdText, conn))
                        {
                            cmd.CommandTimeout = 120; // Hạn chờ 2 phút cho các giao dịch lớn
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show(
                    "Chào mừng bạn đến với LaptopAZ!\n\n" +
                    "Hệ thống phát hiện Cơ sở dữ liệu chưa được khởi tạo cục bộ trên máy tính của bạn.\n" +
                    "Đã tự động khởi tạo Cơ sở dữ liệu 'LaptopAZDB' và nạp toàn bộ cấu trúc + dữ liệu mẫu thành công!\n\n" +
                    "Nhấn OK để bắt đầu ứng dụng.",
                    "Khởi tạo CSDL Thành Công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "LỖI TỰ ĐỘNG KHỞI TẠO CƠ SỞ DỮ LIỆU CỤC BỘ:\n\n" +
                    ex.Message + "\n\n" +
                    "Vui lòng đảm bảo rằng SQL Server LocalDB đã được cài đặt và đang chạy bình thường trên máy tính của bạn (Server=(localdb)\\MSSQLLocalDB).",
                    "Lỗi hệ thống khởi chạy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        private static string FindSqlScriptPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Định nghĩa các đường dẫn tương đối có thể chứa tệp tin SQL
            string[] paths = new[]
            {
                Path.Combine(baseDir, "SQL", "LaptopAZDatabase.sql"),
                Path.Combine(baseDir, "LaptopAZDatabase.sql"),
                Path.Combine(baseDir, "..", "..", "..", "SQL", "LaptopAZDatabase.sql"),
                Path.Combine(baseDir, "..", "..", "SQL", "LaptopAZDatabase.sql")
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }
    }
}
