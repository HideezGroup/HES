using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class OrgStructureService : IOrgStructureService, IDisposable
    {
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;
        private readonly IAsyncRepository<Position> _positionRepository;

        public OrgStructureService(IAsyncRepository<Company> companyRepository,
                                   IAsyncRepository<Department> departmentRepository,
                                   IAsyncRepository<Position> positionRepository)
        {
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
        }

        #region Company

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public async Task<Company> GetCompanyByIdAsync(string id)
        {
            return await _companyRepository.GetByIdAsync(id);
        }

        public async Task<List<Company>> GetCompaniesAsync()
        {
            return await _companyRepository.Query().Include(x => x.Departments).OrderBy(c => c.Name).AsNoTracking().ToListAsync();
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            var exist = await _companyRepository.Query().AsNoTracking().AnyAsync(x => x.Name == company.Name);
            if (exist)
                throw new AlreadyExistException("Already in use.");

            return await _companyRepository.AddAsync(company);
        }

        public async Task EditCompanyAsync(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            var exist = await _companyRepository.Query().AsNoTracking().AnyAsync(x => x.Name == company.Name && x.Id != company.Id);
            if (exist)
                throw new AlreadyExistException("Already in use.");

            await _companyRepository.UpdateAsync(company);
        }

        public async Task UnchangedCompanyAsync(Company company)
        {
            await _companyRepository.UnchangedAsync(company);
        }

        public async Task DeleteCompanyAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                throw new Exception("Company does not exist.");
            }
            await _companyRepository.DeleteAsync(company);
        }

        #endregion

        #region Department

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _departmentRepository
                .Query()
                .Include(d => d.Company)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Department>> GetDepartmentsByCompanyIdAsync(string id)
        {
            return await _departmentRepository
                .Query()
                .Include(d => d.Company)
                .Where(d => d.CompanyId == id)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(string id)
        {
            return await _departmentRepository
                .Query()
                .Include(d => d.Company)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            var exist = await _departmentRepository.Query().AsNoTracking().AnyAsync(x => x.Name == department.Name);
            if (exist)
                throw new AlreadyExistException("Already in use.");

            return await _departmentRepository.AddAsync(department);
        }

        public async Task<Department> TryAddAndGetDepartmentWithCompanyAsync(string companyName, string departmentName)
        {
            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(departmentName))
                return null;

            var department = await _departmentRepository
                .Query()
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Name == departmentName && x.Company.Name == companyName);

            if (department != null)
                return department;

            var company = await _companyRepository
                .Query()
                .FirstOrDefaultAsync(x => x.Name == companyName);

            if (company == null)
                company = await _companyRepository.AddAsync(new Company() { Name = companyName });

            return await _departmentRepository.AddAsync(new Department() { Name = departmentName, CompanyId = company.Id });
        }

        public async Task EditDepartmentAsync(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            var exist = await _departmentRepository.Query().AsNoTracking().AnyAsync(x => x.Name == department.Name && x.Id != department.Id);
            if (exist)
                throw new AlreadyExistException("Already in use.");

            await _departmentRepository.UpdateAsync(department);
        }

        public async Task UnchangedDepartmentAsync(Department department)
        {
            await _departmentRepository.UnchangedAsync(department);
        }

        public async Task DeleteDepartmentAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                throw new Exception("Department does not exist.");

            await _departmentRepository.DeleteAsync(department);
        }

        #endregion

        #region Position

        public IQueryable<Position> PositionQuery()
        {
            return _positionRepository.Query();
        }

        public async Task<List<Position>> GetPositionsAsync()
        {
            return await _positionRepository
                .Query()
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Position> GetPositionByIdAsync(string id)
        {
            return await _positionRepository.GetByIdAsync(id);
        }

        public async Task<Position> CreatePositionAsync(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            var exist = await _positionRepository.Query().AsNoTracking().AnyAsync(x => x.Name == position.Name);
            if (exist)
                throw new AlreadyExistException("Already in use.");

            return await _positionRepository.AddAsync(position);
        }

        public async Task<Position> TryAddAndGetPositionAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var position = await _positionRepository.Query().FirstOrDefaultAsync(x => x.Name == name);

            if (position != null)
                return position;

            return await _positionRepository.AddAsync(new Position() { Name = name });
        }

        public async Task EditPositionAsync(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }

            var exist = await _positionRepository.Query().AsNoTracking().AnyAsync(x => x.Name == position.Name && x.Id != position.Id);
            if (exist)
                throw new AlreadyExistException("Already in use.");

            await _positionRepository.UpdateAsync(position);
        }

        public async Task UnchangedPositionAsync(Position position)
        {
            await _positionRepository.UnchangedAsync(position);
        }

        public async Task DeletePositionAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null)
                throw new Exception("Position does not exist.");

            await _positionRepository.DeleteAsync(position);
        }

        #endregion

        public void Dispose()
        {
            _companyRepository.Dispose();
            _departmentRepository.Dispose();
            _positionRepository.Dispose();
        }
    }
}