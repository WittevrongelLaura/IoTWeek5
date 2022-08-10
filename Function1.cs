using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using IoTWeek5.models;
using System.Data.SqlClient;

namespace IoTWeek5
{
    public class Function1
    {
        //ophalen steden
        [FunctionName("GetCities")]
        public async Task<IActionResult> GetCities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/cities")] HttpRequest req,
            ILogger log)
        {

            try
            {
                List<City> listCities = new List<City>();
                using(SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();
                    using(SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM city";
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while(await reader.ReadAsync())
                        {
                            listCities.Add(new City()
                            {
                                CityId = reader["CityId"].ToString(),
                                Name = reader["Name"].ToString()
                            }); 
                        }
                    }
                }
                return new OkObjectResult(listCities);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }  
        }


        //ophalen afval categorieen
        [FunctionName("GetGarbageTypes")]
        public async Task<IActionResult> GetGarbageTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/garbagetypes")] HttpRequest req,
            ILogger log)
        {

            try
            {
                List<GarbageCategory> listGarbageTypes = new List<GarbageCategory>();
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM garbageType";
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            listGarbageTypes.Add(new GarbageCategory()
                            {
                                GarbageTypeId = reader["GarbageTypeId"].ToString(),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
                return new OkObjectResult(listGarbageTypes);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }
        }

        //toevoegen registraties
        [FunctionName("AddRegistration")]
        public async Task<IActionResult> GetRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/registration")] HttpRequest req,
            ILogger log)
        {

            try
            {
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var registration = JsonConvert.DeserializeObject<Registration>(json);

                string registrationId = Guid.NewGuid().ToString();
                registration.GarbageRegistrationId = registrationId;

                using(SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();

                    using(SqlCommand command = new SqlCommand())
                    {
                        log.LogInformation(DateTime.Now.ToString());
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO GarbageRegistration VALUES (@GarbageRegistrationId, @Name, @EMail, @Description, @GarbageTypeId, @CityId, @Street, " +
                            "@Weight, @Lat, @Long, CURRENT_TIMESTAMP)";
                        command.Parameters.AddWithValue("@GarbageRegistrationId", registration.GarbageRegistrationId);
                        command.Parameters.AddWithValue("@Name", registration.Name);
                        command.Parameters.AddWithValue("@EMail", registration.EMail);
                        command.Parameters.AddWithValue("@Description", registration.Description);
                        command.Parameters.AddWithValue("@GarbageTypeId", registration.GarbageTypeId);
                        command.Parameters.AddWithValue("@CityId", registration.CityId);
                        command.Parameters.AddWithValue("@Street", registration.Street);
                        command.Parameters.AddWithValue("@Weight", registration.Weight);
                        command.Parameters.AddWithValue("@Lat", registration.Lat);
                        command.Parameters.AddWithValue("@Long", registration.Long);
                        //command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                
                return new OkObjectResult(registration);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }
        }

        //ophalen van al mijn registraties
        [FunctionName("GetAllRegistrations")]
        public async Task<IActionResult> GetAllRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                List<Registration> listRegistrations = new List<Registration>();
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM GarbageRegistration";
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            listRegistrations.Add(new Registration()
                            {
                                GarbageRegistrationId = reader["GarbageRegistrationId"].ToString(),
                                Name = reader["Name"].ToString(),
                                EMail = reader["EMail"].ToString(),
                                Description = reader["Description"].ToString(),
                                GarbageTypeId = reader["GarbageTypeId"].ToString(),
                                CityId = reader["CityId"].ToString(),
                                Street = reader["Street"].ToString(),
                                Weight = float.Parse(reader["Weight"].ToString()),
                                Lat = float.Parse(reader["Lat"].ToString()),
                                Long = float.Parse(reader["Long"].ToString()),
                                Timestamp = DateTime.Parse(reader["Timestamp"].ToString())
                            });
                        }
                    }
                }
                return new OkObjectResult(listRegistrations);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }
        }

        //ophalen van 1 registratie
        [FunctionName("GetMyRegistrations")]
        public async Task<IActionResult> GetMyRegistrations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/registration/{email}")] HttpRequest req, string email,
            ILogger log)
        {
            try
            {
                List<Registration> listRegistrations = new List<Registration>();
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM GarbageRegistration WHERE EMail = @email";
                        command.Parameters.AddWithValue("@email", email);
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            listRegistrations.Add(new Registration()
                            {
                                GarbageRegistrationId = reader["GarbageRegistrationId"].ToString(),
                                Name = reader["Name"].ToString(),
                                EMail = reader["EMail"].ToString(),
                                Description = reader["Description"].ToString(),
                                GarbageTypeId = reader["GarbageTypeId"].ToString(),
                                CityId = reader["CityId"].ToString(),
                                Street = reader["Street"].ToString(),
                                Weight = float.Parse(reader["Weight"].ToString()),
                                Lat = float.Parse(reader["Lat"].ToString()),
                                Long = float.Parse(reader["Long"].ToString()),
                                Timestamp = DateTime.Parse(reader["Timestamp"].ToString())
                            });
                        }
                    }
                }
                return new OkObjectResult(listRegistrations);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }
        }

        //verwijderen van 1 registratie
        [FunctionName("DeleteRegistration")]
        public async Task<IActionResult> DeleteRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/registration/{id}")] HttpRequest req, string id,
            ILogger log)
        {
            try
            {
                List<Registration> listRegistrations = new List<Registration>();
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "DELETE FROM GarbageRegistration WHERE GarbageRegistrationId = @id";
                        command.Parameters.AddWithValue("@id", id);
                        //command.Parameters.AddWithValue("@email", email);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                return new OkObjectResult(listRegistrations);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }
        }

        //wijzigen van 1 registratie
        [FunctionName("UpdateRegistration")]
        public async Task<IActionResult> UpdateRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "update", Route = "v1/registration/{id}")] HttpRequest req, string id,
            ILogger log)
        {

            try
            {
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var registration = JsonConvert.DeserializeObject<Registration>(json);

                string registrationId = Guid.NewGuid().ToString();
                registration.GarbageRegistrationId = registrationId;

                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SQLConnectionString")))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "UPDATE GarbageRegistration SET Name = @Name, EMail = @EMail, Description = @Description, GarbageTypeId = @GarbageTypeId," +
                            "CityId = @CityId, Street = @Street, Weight = @Weight, Lat = @Lat, Long = @Long, Timestamp = @Timestamp) WHERE GarbageREgistrationId = @id";
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@Name", registration.Name);
                        command.Parameters.AddWithValue("@EMail", registration.EMail);
                        command.Parameters.AddWithValue("@Description", registration.Description);
                        command.Parameters.AddWithValue("@GarbageTypeId", registration.GarbageTypeId);
                        command.Parameters.AddWithValue("@CityId", registration.CityId);
                        command.Parameters.AddWithValue("@Street", registration.Street);
                        command.Parameters.AddWithValue("@Weight", registration.Weight);
                        command.Parameters.AddWithValue("@Lat", registration.Lat);
                        command.Parameters.AddWithValue("@Long", registration.Long);
                        command.Parameters.AddWithValue("@Timestamp", registration.Timestamp);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return new OkObjectResult(registration);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new StatusCodeResult(500);
            }
        }
    }
}
