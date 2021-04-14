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
    public class OrgStructureService : IOrgStructureService
    {
        private readonly IApplicationDbContext _dbContext;

        public OrgStructureService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Company

        public async Task<Company> GetCompanyByIdAsync(string companyId)
        {
            return await _dbContext.Companies.FindAsync(companyId);
        }

        public async Task<List<Company>> GetCompaniesAsync()
        {
            return await _dbContext.Companies.Include(x => x.Departments).OrderBy(c => c.Name).AsNoTracking().ToListAsync();
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            var exist = await _dbContext.Companies.AsNoTracking().AnyAsync(x => x.Name == company.Name);
            if (exist)
            {
                throw new HESException(HESCode.CompanyNameAlreadyInUse);
            }

            var result = _dbContext.Companies.Add(company);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task EditCompanyAsync(Company company)
        {
            if (company == null)
            {
                throw new ArgumentNullException(nameof(company));
            }

            var exist = await _dbContext.Companies.AsNoTracking().AnyAsync(x => x.Name == company.Name && x.Id != company.Id);
            if (exist)
            {
                throw new HESException(HESCode.CompanyNameAlreadyInUse);
            }

            _dbContext.Companies.Update(company);
            await _dbContext.SaveChangesAsync();
        }

        public void UnchangedCompany(Company company)
        {
            _dbContext.Unchanged(company);
        }

        public async Task DeleteCompanyAsync(string companyId)
        {
            if (companyId == null)
            {
                throw new ArgumentNullException(nameof(companyId));
            }

            var company = await _dbContext.Companies.FindAsync(companyId);
            if (company == null)
            {
                throw new HESException(HESCode.CompanyNotFound);
            }
            
            _dbContext.Companies.Remove(company);
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Department

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _dbContext.Departments
                .Include(d => d.Company)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Department>> GetDepartmentsByCompanyIdAsync(string companyId)
        {
            return await _dbContext.Departments
                .Include(d => d.Company)
                .Where(d => d.CompanyId == companyId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(string departmentId)
        {
            return await _dbContext.Departments
                .Include(d => d.Company)
                .FirstOrDefaultAsync(m => m.Id == departmentId);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            var exist = await _dbContext.Departments.AsNoTracking().AnyAsync(x => x.Name == department.Name);
            if (exist)
            {
                throw new HESException(HESCode.DepartmentNameAlreadyInUse);
            }

            var result = _dbContext.Departments.Add(department);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Department> TryAddAndGetDepartmentWithCompanyAsync(string companyName, string departmentName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                throw new ArgumentNullException(nameof(companyName));
            }

            if (string.IsNullOrWhiteSpace(departmentName))
            {
                throw new ArgumentNullException(nameof(departmentName));
            }

            var department = await _dbContext.Departments
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Name == departmentName && x.Company.Name == companyName);

            if (department != null)
            {
                return department;
            }

            var company = await _dbContext.Companies
                .FirstOrDefaultAsync(x => x.Name == companyName);

            if (company == null)
            {
                var companyResult = _dbContext.Companies.Add(new Company() { Name = companyName });
                await _dbContext.SaveChangesAsync();
                company = companyResult.Entity;
            }

            var result = _dbContext.Departments.Add(new Department() { Name = departmentName, CompanyId = company.Id });
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task EditDepartmentAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            var exist = await _dbContext.Departments.AsNoTracking().AnyAsync(x => x.Name == department.Name && x.Id != department.Id);
            if (exist)
            {
                throw new HESException(HESCode.DepartmentNameAlreadyInUse);
            }

            _dbContext.Departments.Update(department);
            await _dbContext.SaveChangesAsync();
        }

        public void UnchangedDepartment(Department department)
        {
            _dbContext.Unchanged(department);
        }

        public async Task DeleteDepartmentAsync(string departmentId)
        {
            if (string.IsNullOrWhiteSpace(departmentId))
            {
                throw new ArgumentNullException(nameof(departmentId));
            }

            var department = await GetDepartmentByIdAsync(departmentId);
            if (department == null)
            {
                throw new HESException(HESCode.DepartmentNotFound);
            }

            _dbContext.Departments.Remove(department);
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Position

        public async Task<List<Position>> GetPositionsAsync()
        {
            return await _dbContext.Positions
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Position> GetPositionByIdAsync(string positionId)
        {
            return await _dbContext.Positions.FindAsync(positionId);
        }

        public async Task<Position> CreatePositionAsync(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }

            var exist = await _dbContext.Positions.AsNoTracking().AnyAsync(x => x.Name == position.Name);
            if (exist)
            {
                throw new HESException(HESCode.PositionNameAlreadyInUse);
            }

            var result = _dbContext.Positions.Add(position);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<Position> TryAddAndGetPositionAsync(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                throw new ArgumentNullException(nameof(positionName));
            }

            var position = await _dbContext.Positions.FirstOrDefaultAsync(x => x.Name == positionName);
            if (position != null)
            {
                return position;
            }

            var result = _dbContext.Positions.Add(new Position() { Name = positionName });
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task EditPositionAsync(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }

            var exist = await _dbContext.Positions.AsNoTracking().AnyAsync(x => x.Name == position.Name && x.Id != position.Id);
            if (exist)
            {
                throw new HESException(HESCode.PositionNameAlreadyInUse);
            }

            _dbContext.Positions.Update(position);
            await _dbContext.SaveChangesAsync();
        }

        public void UnchangedPosition(Position position)
        {
            _dbContext.Unchanged(position);
        }

        public async Task DeletePositionAsync(string positionId)
        {
            if (string.IsNullOrWhiteSpace(positionId))
            {
                throw new ArgumentNullException(nameof(positionId));
            }

            var position = await _dbContext.Positions.FindAsync(positionId);
            if (position == null)
            {
                throw new HESException(HESCode.PositionNotFound);
            }

            _dbContext.Positions.Remove(position);
            await _dbContext.SaveChangesAsync();
        }

        #endregion
    }
}