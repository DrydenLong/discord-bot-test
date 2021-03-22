using Dapper;
using Library.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Service
{
    public interface IDatabaseService
    {
        bool CreateCustomResponse(string command, string response);
        bool RemoveCustomResponse(string command);
        List<CustomResponse> GetCustomResponses(string command);
        List<CustomResponse> GetAllCustomResponses();
        List<User> GetAdmins();
        bool AddAdmin(int userId, bool superAdmin);
        bool RemoveAdmin(int userId);
    }
    public class DatabaseService : IDatabaseService
    {
        private readonly Config _config;

        public DatabaseService(Config config)
        {
            _config = config;
        }

        public bool CreateCustomResponse(string command, string response)
        {
            try
            {
                var sql = "INSERT INTO CustomResponses (Command, Response) VALUES (@Command, @Response);";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    var result = conn.Execute(sql, new { Command = command, Response = response });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        public bool RemoveCustomResponse(string command)
        {
            try
            {
                var sql = "DELETE FROM CustomResponses WHERE Command = @Command;";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    var result = conn.Execute(sql, new { Command = command });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        public List<CustomResponse> GetCustomResponses(string command)
        {
            try
            {
                var sql = "SELECT * FROM CustomResponses WHERE Command = @Command;";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    return conn.Query<CustomResponse>(sql, new {Command = command}).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new List<CustomResponse>();
        }

        public List<CustomResponse> GetAllCustomResponses()
        {
            try
            {
                var sql = "SELECT * FROM CustomResponses;";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    return conn.Query<CustomResponse>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new List<CustomResponse>();
        }

        public List<User> GetAdmins()
        {
            try
            {
                var sql = "SELECT * FROM admins;";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    return conn.Query<User>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new List<User>();
        }

        public bool AddAdmin(int userId, bool superAdmin)
        {
            try
            {
                var sql = "INSERT INTO Admins (UserId, SuperAdmin, Active) VALUES (@UserId, @SuperAdmin, true);";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    conn.Execute(sql, new { UserId = userId, SuperAdmin = superAdmin });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        public bool RemoveAdmin(int userId)
        {
            try
            {
                var sql = "UPDATE Admins SET Active = false WHERE UserId = @UserId";
                using (var conn = new NpgsqlConnection(_config.DatabaseConnectionString))
                {
                    conn.Execute(sql, new { UserId = userId });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }
    }
}
