using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Core.Models.SoftwareVault;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SoftwareVaultService : ISoftwareVaultService, IDisposable
    {
        private readonly IAsyncRepository<SoftwareVault> _softwareVaultRepository;
        private readonly IAsyncRepository<SoftwareVaultInvitation> _softwareVaultInvitationRepository;
        private readonly IEmailSenderService _emailSenderService;

        public SoftwareVaultService(IAsyncRepository<SoftwareVault> softwareVaultRepository,
                                    IAsyncRepository<SoftwareVaultInvitation> softwareVaultInvitationRepository,
                                    IEmailSenderService emailSenderService)
        {
            _softwareVaultRepository = softwareVaultRepository;
            _softwareVaultInvitationRepository = softwareVaultInvitationRepository;
            _emailSenderService = emailSenderService;
        }

        public IQueryable<SoftwareVault> SoftwareVaultQuery()
        {
            return _softwareVaultRepository.Query();
        }

        public async Task<List<SoftwareVault>> GetSoftwareVaultsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, SoftwareVaultFilter filter)
        {
            var query = _softwareVaultRepository
               .Query()
               .Include(x => x.Employee.Department.Company)
               .AsQueryable();

            // Filter
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.OS))
                {
                    query = query.Where(x => x.OS.Contains(filter.OS, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(filter.Model))
                {
                    query = query.Where(x => x.Model.Contains(filter.Model, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(filter.ClientAppVersion))
                {
                    query = query.Where(x => x.ClientAppVersion.Contains(filter.ClientAppVersion, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Status != null)
                {
                    query = query.Where(x => x.Status == filter.Status);
                }
                if (filter.LicenseStatus != null)
                {
                    query = query.Where(x => x.LicenseStatus == filter.LicenseStatus);
                }
                if (filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x => x.OS.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.ClientAppVersion.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (sortColumn)
            {
                case nameof(SoftwareVault.OS):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OS) : query.OrderByDescending(x => x.OS);
                    break;
                case nameof(SoftwareVault.Model):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Model) : query.OrderByDescending(x => x.Model);
                    break;
                case nameof(SoftwareVault.ClientAppVersion):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.ClientAppVersion) : query.OrderByDescending(x => x.ClientAppVersion);
                    break;
                case nameof(SoftwareVault.Status):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status);
                    break;
                case nameof(SoftwareVault.LicenseStatus):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseStatus) : query.OrderByDescending(x => x.LicenseStatus);
                    break;
                case nameof(SoftwareVault.Employee):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(SoftwareVault.Employee.Department.Company):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Company.Name) : query.OrderByDescending(x => x.Employee.Department.Company.Name);
                    break;
                case nameof(SoftwareVault.Employee.Department):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Name) : query.OrderByDescending(x => x.Employee.Department.Name);
                    break;
            }

            return await query.Skip(skip).Take(take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetVaultsCountAsync(string searchText, SoftwareVaultFilter filter)
        {
            var query = _softwareVaultRepository.Query();

            // Filter
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.OS))
                {
                    query = query.Where(x => x.OS.Contains(filter.OS, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(filter.Model))
                {
                    query = query.Where(x => x.Model.Contains(filter.Model, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(filter.ClientAppVersion))
                {
                    query = query.Where(x => x.ClientAppVersion.Contains(filter.ClientAppVersion, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Status != null)
                {
                    query = query.Where(x => x.Status == filter.Status);
                }
                if (filter.LicenseStatus != null)
                {
                    query = query.Where(x => x.LicenseStatus == filter.LicenseStatus);
                }
                if (filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x => x.OS.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.ClientAppVersion.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public IQueryable<SoftwareVaultInvitation> SoftwareVaultInvitationQuery()
        {
            return _softwareVaultInvitationRepository.Query();
        }

        public async Task<List<SoftwareVaultInvitation>> GetSoftwareVaultInvitationsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, SoftwareVaultInvitationFilter filter)
        {
            var query = _softwareVaultInvitationRepository
             .Query()
             .Include(x => x.Employee.Department.Company)
             .AsQueryable();

            // Filter
            if (filter != null)
            {
                if (filter.Id != null)
                {
                    query = query.Where(x => x.Id.Contains(filter.Id, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.CreatedAtStartDate != null)
                {
                    query = query.Where(x => x.CreatedAt >= filter.CreatedAtStartDate.Value.Date.ToUniversalTime());
                }
                if (filter.CreatedAtEndDate != null)
                {
                    query = query.Where(x => x.CreatedAt <= filter.CreatedAtEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (filter.ValidToStartDate != null)
                {
                    query = query.Where(x => x.ValidTo >= filter.ValidToStartDate.Value.Date);
                }
                if (filter.ValidToEndDate != null)
                {
                    query = query.Where(x => x.ValidTo <= filter.ValidToEndDate.Value.Date);
                }
                if (filter.Status != null)
                {
                    query = query.Where(x => x.Status == filter.Status);
                }
                if (!string.IsNullOrWhiteSpace(filter.Email))
                {
                    query = query.Where(x => x.Employee.Email.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x => x.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (sortColumn)
            {
                case nameof(SoftwareVaultInvitation.Id):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                    break;
                case nameof(SoftwareVaultInvitation.CreatedAt):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
                    break;
                case nameof(SoftwareVaultInvitation.ValidTo):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.ValidTo) : query.OrderByDescending(x => x.ValidTo);
                    break;
                case nameof(SoftwareVaultInvitation.Status):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status);
                    break;
                case nameof(SoftwareVaultInvitation.Employee.Email):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Email) : query.OrderByDescending(x => x.Employee.Email);
                    break;
                case nameof(SoftwareVault.Employee):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(SoftwareVault.Employee.Department.Company):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Company.Name) : query.OrderByDescending(x => x.Employee.Department.Company.Name);
                    break;
                case nameof(SoftwareVault.Employee.Department):
                    query = sortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Name) : query.OrderByDescending(x => x.Employee.Department.Name);
                    break;
            }

            return await query.Skip(skip).Take(take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetInvitationsCountAsync(string searchText, SoftwareVaultInvitationFilter filter)
        {
            var query = _softwareVaultInvitationRepository.Query();

            // Filter
            if (filter != null)
            {
                if (filter.Id != null)
                {
                    query = query.Where(x => x.Id.Contains(filter.Id, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.CreatedAtStartDate != null)
                {
                    query = query.Where(x => x.CreatedAt >= filter.CreatedAtStartDate.Value.Date);
                }
                if (filter.CreatedAtEndDate != null)
                {
                    query = query.Where(x => x.CreatedAt <= filter.CreatedAtEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (filter.ValidToStartDate != null)
                {
                    query = query.Where(x => x.ValidTo >= filter.ValidToStartDate.Value.Date);
                }
                if (filter.ValidToEndDate != null)
                {
                    query = query.Where(x => x.ValidTo <= filter.ValidToEndDate.Value.Date);
                }
                if (filter.Status != null)
                {
                    query = query.Where(x => x.Status == filter.Status);
                }
                if (!string.IsNullOrWhiteSpace(filter.Email))
                {
                    query = query.Where(x => x.Employee.Email.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(filter.Department, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim();

                query = query.Where(x => x.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task CreateAndSendInvitationAsync(Employee employee, ServerSettings server, DateTime validTo)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.Id == null)
                throw new ArgumentNullException(nameof(employee.Id));

            if (employee.Email == null)
                throw new ArgumentNullException(nameof(employee.Email));

            var activationCode = GenerateActivationCode();

            var invitation = new SoftwareVaultInvitation()
            {
                EmployeeId = employee.Id,
                Status = SoftwareVaultInvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ValidTo = validTo.Date,
                AcceptedAt = null,
                SoftwareVaultId = null,
                ActivationCode = activationCode
            };

            var created = await _softwareVaultInvitationRepository.AddAsync(invitation);

            var activation = new SoftwareVaultActivation()
            {
                ServerAddress = server.Url,
                ActivationId = created.Id,
                ActivationCode = activationCode
            };

            await _emailSenderService.SendSoftwareVaultInvitationAsync(employee, activation, validTo);
        }

        private int GenerateActivationCode()
        {
            return new Random().Next(100000, 999999);
        }

        public async Task ResendInvitationAsync(Employee employee, ServerSettings server, string invitationId)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (server == null)
                throw new ArgumentNullException(nameof(server));

            if (invitationId == null)
                throw new ArgumentNullException(nameof(invitationId));

            var invitation = await _softwareVaultInvitationRepository.GetByIdAsync(invitationId);
            if (invitation == null)
                throw new NotFoundException("Invitation not found.");

            var activationCode = GenerateActivationCode();

            invitation.Status = SoftwareVaultInvitationStatus.Pending;
            invitation.ValidTo = DateTime.Now.AddDays(2);
            invitation.ActivationCode = activationCode;

            var prop = new string[]
            {
                nameof(SoftwareVaultInvitation.Status),
                nameof(SoftwareVaultInvitation.ValidTo),
                nameof(SoftwareVaultInvitation.ActivationCode)
            };

            await _softwareVaultInvitationRepository.UpdateOnlyPropAsync(invitation, prop);

            var activation = new SoftwareVaultActivation()
            {
                ServerAddress = server.Url,
                ActivationId = invitation.Id,
                ActivationCode = activationCode
            };

            await _emailSenderService.SendSoftwareVaultInvitationAsync(employee, activation, invitation.ValidTo);
        }
        
        public async Task<SoftwareVaultInvitation> DeleteInvitationAsync(string invitationId)
        {
            if (invitationId == null)
                throw new ArgumentNullException(nameof(invitationId));

            var invitation = await _softwareVaultInvitationRepository.GetByIdAsync(invitationId);
            if (invitation == null)
                throw new NotFoundException("Invitation not found.");

            return await _softwareVaultInvitationRepository.DeleteAsync(invitation);
        }

        public void Dispose()
        {
            _softwareVaultRepository.Dispose();
            _softwareVaultInvitationRepository.Dispose();
        }
    }
}