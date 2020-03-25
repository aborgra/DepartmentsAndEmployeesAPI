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

    
}
}