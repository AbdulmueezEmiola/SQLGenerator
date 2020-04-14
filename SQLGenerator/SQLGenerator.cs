using SQLGenerator.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Text;

namespace SQLGenerator
{
    public static class SqlGenerator
    {
        public static void CreateTable(Type t)
        {

            SQLiteConnection con;
            SQLiteCommand cmd;
            SQLiteDataReader dr;
            if (!File.Exists("SQLGenerator.sqlite"))
            {
                SQLiteConnection.CreateFile("SQLGenerator.sqlite");
            }            
            string sql = GetCreateMethod(t);
            con = new SQLiteConnection("Data Source = SQLGenerator.sqlite; Version=3;");
            con.Open();
            cmd = new SQLiteCommand(sql, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static void AddData(Object obj)
        {
            SQLiteConnection con = new SQLiteConnection("Data Source = SQLGenerator.sqlite; Version=3;");
            SQLiteCommand cmd;
            cmd = new SQLiteCommand();
            con.Open();
            cmd.CommandText = GetAdderMethod(obj);
            cmd.Connection = con;
            cmd.ExecuteNonQuery();
            con.Close();
        }
        public static List<T> SelectData<T>()
        {
            T data = default(T);
            List<T> list = new List<T>();
            SQLiteConnection con = new SQLiteConnection("Data Source = SQLGenerator.sqlite; Version=3;");
            SQLiteDataReader dr;
            SQLiteCommand cmd;
            cmd = new SQLiteCommand(con);
            con.Open();
            cmd.CommandText = GetSelectMethod(typeof(T));
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                data = Activator.CreateInstance<T>();
                foreach(PropertyInfo prop in data.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        if (prop.PropertyType.ToString() == "System.Int32")
                        {
                            prop.SetValue(data, Convert.ToInt32(dr[prop.Name]), null);
                        }
                        else
                        {
                            prop.SetValue(data, dr[prop.Name], null);
                        }
                    }
                }
                list.Add(data);
            }
            return list;
        }
        private static string GetSelectMethod(Type t)
        {
            Table tableAttribute = (Table)Attribute.GetCustomAttribute(t, typeof(Table));
            if(tableAttribute == null)
            {
                throw new AttributeException("No attribute in class");
            }
            string selectTable = "Select * From " + tableAttribute.name.ToUpper();
            return selectTable;
        } 
        private static string GetAdderMethod(Object obj)
        {
            List<string> columnHeaders = new List<string>();
            var t = obj.GetType();
            Table tableAttribute = (Table)Attribute.GetCustomAttribute(t, typeof(Table));
            if (tableAttribute == null)
            {
                throw new AttributeException("No attribute in class");
            }
            string value = "";
            PropertyInfo[] properties = obj.GetType().GetProperties();
            for (int i = 0; i < properties.Length; ++i)
            {
                Column colAttr = (Column)Attribute.GetCustomAttribute(properties[i], typeof(Column));
                string val = properties[i].PropertyType.ToString() == "System.String" ? "\'" + properties[i].GetValue(obj).ToString() + "\'" : properties[i].GetValue(obj).ToString();
                value += i == 0 ? val: ", " + val;
                columnHeaders.Add(colAttr.name);
            }
            string template = "insert into " + tableAttribute.name.ToUpper() + "(" + string.Join(", ", columnHeaders.ToArray()) + ") VALUES(";
            string adder = template + value + ")";
            return adder;
        }
        
        private static string GetCreateMethod(Type t)
        {
            Table tableAttribute = (Table)Attribute.GetCustomAttribute(t, typeof(Table));
            if (tableAttribute == null)
            {
                throw new AttributeException("No attribute in class");
            }
            string createTable = "create table if not exists " + tableAttribute.name.ToUpper() + "(";
            PropertyInfo[] myMembers = t.GetProperties();
            for (int i = 0; i < myMembers.Length; i++)
            {
                Column colAttr = (Column)Attribute.GetCustomAttribute(myMembers[i], typeof(Column));
                createTable += i == 0 ? colAttr.name + " " : ", " + colAttr.name + " ";
                createTable += getTypeOfProperty(myMembers[i].PropertyType.ToString()) + " ";
                createTable += colAttr.primaryKey == true ? "PRIMARY KEY" : "";
                createTable += colAttr.NotNull == true ? "NOT NULL" : "";
                createTable += colAttr.unique == true ? "UNIQUE" : "";
            }
            createTable += ")";
            return createTable;
        }
        private static string getTypeOfProperty(string t)
        {
            switch (t)
            {
                case "System.Int32":
                    return "INTEGER";
                case "System.String":
                    return "VARCHAR2";
                case "System.Decimal":
                    return "DECIMAL";
                default:
                    return "VARCHAR";
            }
        }
        
    }
}
