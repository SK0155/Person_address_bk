using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using reactpersonaddress.Models;

namespace reactpersonaddress.Controller
{

    [ApiController]
    [Route("api/Data")]
    public class DataController : ControllerBase
    {
        private readonly string _connectionString;

        // Inject IConfiguration to access the connection string
        public DataController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Endpoint to add data to the Person table
        [HttpPost("person")]
        public IActionResult AddPerson([FromBody] Person person)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Check if the email already exists
                string checkQuery = "SELECT COUNT(*) FROM Person WHERE Email = @Email";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", person.Email);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                // If the email already exists, return a conflict response
                if (count > 0)
                {
                    return Conflict(new { message = "duplicate email" });
                }

                // If no duplicate, insert the new person
                string insertQuery = "INSERT INTO Person (Name, Age, Email) VALUES (@Name, @Age, @Email)";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@Name", person.Name);
                insertCmd.Parameters.AddWithValue("@Age", person.Age);
                insertCmd.Parameters.AddWithValue("@Email", person.Email);

                insertCmd.ExecuteNonQuery();
            }

            return Ok(new { message = "Person added successfully" });
        }


        // Endpoint to add data to the Address table
        [HttpPost("address")]
        public IActionResult AddAddress([FromBody] Address address)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Address (Street, City, PostalCode) VALUES (@Street, @City, @PostalCode)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Street", address.Street);
                cmd.Parameters.AddWithValue("@City", address.City);
                cmd.Parameters.AddWithValue("@PostalCode", address.PostalCode);

                cmd.ExecuteNonQuery();
            }
            return Ok(new { message = "Address added successfully" });
        }

        // Endpoint to update data in the Person table

        // PUT method to update a person
        [HttpPut("person/{id}")]
        public IActionResult UpdatePerson(int id, [FromBody] Person person)
        {
            // Check if the request body is empty or the ids don't match
            if (person == null || id != person.PersonId)
            {
                return BadRequest(new { message = "Invalid data. Please check the input." });
            }

            // Log model state errors, if any
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Returns detailed model state errors
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Person SET Name = @Name, Age = @Age, Email = @Email WHERE PersonId = @PersonId";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Add parameters for SQL query
                    cmd.Parameters.AddWithValue("@Name", person.Name);
                    cmd.Parameters.AddWithValue("@Age", person.Age);
                    cmd.Parameters.AddWithValue("@Email", person.Email);
                    cmd.Parameters.AddWithValue("@PersonId", id);

                    // Execute the update query
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        // No rows updated, meaning no matching person found
                        return NotFound(new { message = "Person not found" });
                    }
                }

                // Return success message if person is updated
                return Ok(new { message = "Person updated successfully" });
            }
            catch (Exception ex)
            {
                // Log the error and return internal server error
                return StatusCode(500, new { message = "An error occurred while updating the person", error = ex.Message });
            }
        }


        // Endpoint to delete data from the Person table
        [HttpDelete("person/{id}")]
        public IActionResult DeletePerson(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Person WHERE PersonId = @PersonId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PersonId", id);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Person not found" });
                }
            }
            return Ok(new { message = "Person deleted successfully" });
        }
        [HttpGet("persons")]
        public IActionResult GetAllPersons()
        {
            List<Person> persons = new List<Person>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT PersonId, Name, Age, Email FROM Person";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        var person = new Person
                        {
                            PersonId = reader.GetInt32("PersonId"),
                            Name = reader.GetString("Name"),
                            Age = reader.GetInt32("Age"),
                            Email = reader.GetString("Email")
                        };
                        persons.Add(person);
                    }
                }
            }
            return Ok(persons);
        }
    }

}