using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DepartmentsAndEmployees.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DepartmentsAndEmployees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT e.Id, e.firstName, e.lastName, e.departmentId, d.deptName as 'Department Name' FROM Employee e LEFT JOIN Department d on e.departmentId = d.Id";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Employee> employees = new List<Employee>();

                    Employee employee = null;

                    while (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("Id");
                        int IdValue = reader.GetInt32(idColumnPosition);

                        int firstNameColumnPosition = reader.GetOrdinal("firstName");
                        string firstNameValue = reader.GetString(firstNameColumnPosition);

                        int lastNameColumnPosition = reader.GetOrdinal("lastName");
                        string lastNameValue = reader.GetString(lastNameColumnPosition);

                        int departmentColumnPosition = reader.GetOrdinal("Department Name");
                        string departmentValue = reader.GetString(departmentColumnPosition);

                        int departmentIdColumnPosition = reader.GetOrdinal("departmentId");
                        int departmentIdValue = reader.GetInt32(departmentIdColumnPosition);

                        employee = new Employee
                        {
                            Id = IdValue,
                            FirstName = firstNameValue,
                            LastName = lastNameValue,
                            DepartmentId = departmentIdValue,
                            Department = new Department
                            {
                                DeptName = departmentValue,
                                Id = departmentIdValue
                            }

                        };

                        employees.Add(employee);
                    }
                    reader.Close();

                    return Ok(employees);
                }
            }
        }
        

        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT e.Id, e.firstName, e.lastName, e.departmentId, d.deptName as 'Department Name' FROM Employee e LEFT JOIN Department d on e.departmentId = d.Id WHERE e.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null;

                    if (reader.Read()) { 
                    int idColumnPosition = reader.GetOrdinal("Id");
                    int IdValue = reader.GetInt32(idColumnPosition);

                    int firstNameColumnPosition = reader.GetOrdinal("firstName");
                    string firstNameValue = reader.GetString(firstNameColumnPosition);

                    int lastNameColumnPosition = reader.GetOrdinal("lastName");
                    string lastNameValue = reader.GetString(lastNameColumnPosition);

                    int departmentColumnPosition = reader.GetOrdinal("Department Name");
                    string departmentValue = reader.GetString(departmentColumnPosition);

                    int departmentIdColumnPosition = reader.GetOrdinal("departmentId");
                    int departmentIdValue = reader.GetInt32(departmentIdColumnPosition);

                    employee = new Employee
                    {
                        Id = IdValue,
                        FirstName = firstNameValue,
                        LastName = lastNameValue,
                        DepartmentId = departmentIdValue,
                        Department = new Department
                        {
                            DeptName = departmentValue,
                            Id = departmentIdValue
                        }

                    };
                }

                reader.Close();

                return Ok(employee);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee (firstName, lastName, departmentId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @departmentId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));


                    int newId = (int)cmd.ExecuteScalar();
                    employee.Id = newId;
                    return CreatedAtRoute("GetEmployee", new { id = newId }, employee);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET firstName = @firstName,
                                                lastName = @lastName,
                                                departmentId = @departmentId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Employee WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, firstName, lastName, departmentId
                        FROM Employee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }
}