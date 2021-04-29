using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IOrgStructureService
    {
        Task<Company> GetCompanyByIdAsync(string id);
        Task<List<Company>> GetCompaniesAsync();
        Task<Company> CreateCompanyAsync(Company company);
        Task EditCompanyAsync(Company company);
        void UnchangedCompany(Company company);
        Task DeleteCompanyAsync(string id);


        Task<List<Department>> GetDepartmentsAsync();
        Task<List<Department>> GetDepartmentsByCompanyIdAsync(string id);
        Task<Department> GetDepartmentByIdAsync(string id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<Department> TryAddAndGetDepartmentWithCompanyAsync(string companyName, string departmentName);
        Task EditDepartmentAsync(Department department);
        void UnchangedDepartment(Department department);
        Task DeleteDepartmentAsync(string id);


        Task<List<Position>> GetPositionsAsync();
        Task<Position> GetPositionByIdAsync(string id);
        Task<Position> CreatePositionAsync(Position position);
        Task<Position> TryAddAndGetPositionAsync(string name);
        Task EditPositionAsync(Position position);
        void UnchangedPosition(Position position);
        Task DeletePositionAsync(string id);
    }
}