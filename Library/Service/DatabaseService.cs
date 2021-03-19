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
        List<CustomResponse> GetCustomResponses(string command);
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
                var sql = "DROP TABLE CustomResponses; CREATE TABLE CustomResponses (Command VARCHAR(9999) NOT NULL, Response VARCHAR(9999) NOT NULL); INSERT INTO CustomResponses (Command, Response) VALUES (@Command, @Response);";
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
    }
}
