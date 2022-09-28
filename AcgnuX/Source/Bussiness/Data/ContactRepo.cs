using AcgnuX.Source.Bussiness.Constants;
using AcgnuX.Source.Model;
using AcgnuX.Source.Utils;
using AcgnuX.Source.ViewModel;
using AcgnuX.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgnuX.Source.Bussiness.Data
{
    public class ContactRepo
    {
        public static ContactRepo Instance => new ContactRepo();

        private ContactRepo()
        {

        }

        /// <summary>
        /// 查询所有联系人
        /// </summary>
        /// <param name="kw"></param>
        /// <returns></returns>
        public List<Contact> FindAll(string kw)
        {
            var sql = new StringBuilder("SELECT * FROM contact");
            var arg = new List<SQLiteParameter>();
            if (!string.IsNullOrEmpty(kw))
            {
                sql.Append(" WHERE name LIKE @kw OR uid = @kw");
                arg.Add(new SQLiteParameter("@kw", "%" + kw + "%"));
            }
            sql.Append(" ORDER BY id DESC");
            var dataSet = SQLite.SqlTable(sql.ToString(), arg);
            var resultSet = new List<Contact>();
            foreach (DataRow dataRow in dataSet.Rows)
            {
                var avatar = dataRow["avatar"];
                var avatarBytes = new byte[0];
                if(avatar != DBNull.Value)
                {
                    avatarBytes = (byte []) avatar;
                }
                resultSet.Add(new Contact()
                {
                    Id = Convert.ToInt32(dataRow["id"]),
                    Name = Convert.ToString(dataRow["name"]),
                    Uid = Convert.ToString(dataRow["uid"]),
                    Phone = Convert.ToString(dataRow["phone"]),
                    Platform = (ContactPlatform) EnumLoader.GetByValue(typeof(ContactPlatform), Convert.ToString(dataRow["platform"])),
                    Avatar = avatarBytes
                }) ;
            }
            return resultSet;
        }

        /// <summary>
        /// 异步查询全部
        /// </summary>
        public async Task<List<Contact>> FindAllAsync(string kw)
        {
            return await Task.Run(() =>
            {
                return FindAll(kw);
            });
        }

        /// <summary>
        /// 新增联系人
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public int Add(ContactItemViewModel vm)
        {
            var i = SQLite.ExecuteNonQuery(
                "INSERT INTO contact(id, platform, uid, name, phone, avatar) VALUES ((SELECT IFNULL(MAX(id),0)  + 1 FROM contact), @platform, @uid, @name, @phone, @avatar)", 
                new List<SQLiteParameter>
            {
                new SQLiteParameter("@platform", vm.Platform.ToString()),
                new SQLiteParameter("@uid", vm.Uid),
                new SQLiteParameter("@name", vm.Name),
                new SQLiteParameter("@phone", vm.Phone),
                new SQLiteParameter("@avatar", vm.Avatar)
            });

            if(i  == 0) return i;

            var sLastId = SQLite.sqlone("SELECT LAST_INSERT_ROWID() FROM contact", null);
            return Convert.ToInt32(sLastId);
        }

        /// <summary>
        /// 更新联系人
        /// </summary>
        /// <param name="vm"></param>
        public void Update(ContactItemViewModel vm)
        {
            SQLite.ExecuteNonQuery(
            "UPDATE contact SET platform = @platform, uid = @uid, name = @name, phone= @phone, avatar = @avatar WHERE id = @id",
                new List<SQLiteParameter>
            {
                        new SQLiteParameter("@platform", vm.Platform.ToString()),
                        new SQLiteParameter("@uid", vm.Uid),
                        new SQLiteParameter("@name", vm.Name),
                        new SQLiteParameter("@phone", vm.Phone),
                        new SQLiteParameter("@avatar", vm.Avatar),
                        new SQLiteParameter("@id", vm.Id.GetValueOrDefault())
            });
        }

        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="id"></param>
        public void Del(int id)
        {
            SQLite.ExecuteNonQuery("DELETE FROM contact WHERE id = @id", new List<SQLiteParameter> { new SQLiteParameter("@id", id) });
        }
    }
}
