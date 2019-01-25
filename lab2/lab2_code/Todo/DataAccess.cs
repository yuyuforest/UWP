using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using Todo.Models;
using Todo.ViewModels;
using Windows.UI.Xaml.Media.Imaging;

namespace Todo
{
    public class DataAccess
    {
        SQLiteConnection conn;
        private static DataAccess instance;
        public static DataAccess Instance
        {
            get
            {
                if (instance == null) instance = new DataAccess();
                return instance;
            }
        }

        public DataAccess()
        {
            conn = new SQLiteConnection("todo.db");
            String sql = @"CREATE TABLE IF NOT EXISTS
  				TodoItems (
                Id      VARCHAR(140) PRIMARY KEY NOT NULL,
  				Title    VARCHAR( 140 ),
  				Description    VARCHAR( 140 ),
  				Date VARCHAR( 140 ),
                Completed INT,
                ImageType VARCHAR( 140 )
  				);";
            using (var statement = conn.Prepare(sql))
            {
                statement.Step();
            }
            this.GetAllData();
        }

        public void AddData(String id, String title, String description, DateTimeOffset date, String imageType, bool completed = false)
        {
            try
            {
                using (var custstmt = conn.Prepare("INSERT INTO TodoItems (Id, Title, Description, Date, ImageType, Completed) 	VALUES (?, ?, ?, ?, ?, ?)"))
                {
                    custstmt.Bind(1, id);
                    custstmt.Bind(2, title);
                    custstmt.Bind(3, description);
                    custstmt.Bind(4, date.ToString());
                    custstmt.Bind(5, imageType);
                    custstmt.Bind(6, completed ? 1 : 0);
                    custstmt.Step();
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle error
            }
        }

        //把数据都加入ViewModel
        public void GetAllData()
        {
            using (var statement = conn.Prepare("SELECT Id, Title, Description, Date, Completed, ImageType FROM TodoItems"))
            {
                while(SQLiteResult.ROW == statement.Step())
                {
                    String id = (String)statement[0];
                    String title = (String)statement[1];
                    String description = (String)statement[2];
                    DateTimeOffset date = DateTimeOffset.Parse((String)statement[3]);
                    bool completed = (Int64)statement[4] == 1 ? true : false;
                    String imageType = (String)statement[5];
                    TodoItemViewModels.Instance.AddTodoItem(title, description, date, null, imageType, false, completed, id);
                }
            }
        }

        //用于在用户在编辑界面提交编辑后更新数据（不包括completed）
        public void UpdateData(String id, String title, String description, DateTimeOffset date, String imageType)
        {
            using (var statement = conn.Prepare("UPDATE TodoItems SET Title = ?, Description = ?, Date = ?, ImageType = ? WHERE Id = ?"))
            {
                statement.Bind(1, title);
                statement.Bind(2, description);
                statement.Bind(3, date.ToString());
                statement.Bind(4, imageType);
                statement.Bind(5, id);
                statement.Step();
            }
        }

        //用于在用户点击复选框时更新数据
        public void UpdateCompleted(String id, bool completed)
        {
            String sql = "UPDATE TodoItems SET Completed = ? WHERE Id =?";
            using (var statement = conn.Prepare(sql))
            {
                statement.Bind(1, completed ? 1 : 0);
                statement.Bind(2, id);
                statement.Step();
            }
        }

        public void DeleteData(String id)
        {
            using (var statement = conn.Prepare("Delete From TodoItems Where Id = ?"))
            {
                statement.Bind(1, id);
                statement.Step();
            }
        }

        //搜索
        public StringBuilder Search(String word)
        {
            StringBuilder result = new StringBuilder();
            word = "%" + word + "%";
            using (var statement = conn.Prepare("SELECT Title,Description,Date FROM TodoItems WHERE Title LIKE ? OR Description LIKE ? OR Date LIKE ?"))
            {
                statement.Bind(1, word);
                statement.Bind(2, word);
                statement.Bind(3, word);
                while (SQLiteResult.ROW == statement.Step())
                {
                    result.Append("Title: ").Append((String)statement[0]).Append(" Description: ").Append((String)statement[1]).Append(" Date: ").Append((String)statement[2]).Append("\n");
                }
            }
            return result;
        }
    }
    
}
