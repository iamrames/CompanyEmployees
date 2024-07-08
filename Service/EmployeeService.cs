using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service;

internal sealed class EmployeeService : IEmployeeService
{ 
    private readonly IRepositoryManager _repository; 
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;
    private readonly IEmployeeLinks _employeeLinks;
    public EmployeeService(IRepositoryManager repository, 
        ILoggerManager logger, 
        IMapper mapper,
        IEmployeeLinks employeeLinks)
    { 
        _repository = repository; 
        _logger = logger; 
        _mapper = mapper;
        _employeeLinks = employeeLinks;
    }
    public async Task<(LinkResponse linkResponse, MetaData metaData)> GetEmployeesAsync(Guid companyId, LinkParameters linkParameters, bool trackChanges) 
    {
        if (!linkParameters.EmployeeParameters.ValidAgeRange) throw new MaxAgeRangeBadRequestException();

        await CheckIfItExists(companyId, trackChanges);
        var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, linkParameters.EmployeeParameters, trackChanges); 
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);
        var links = _employeeLinks.TryGenerateLinks(employeesDto, linkParameters.EmployeeParameters.Fields!, companyId, linkParameters.Context);
        return (linkResponse: links, metaData: employeesWithMetaData.MetaData);

    }

    public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges) 
    {
        await CheckIfItExists(companyId, trackChanges);
        var employee = await GetEmployeeAndCheckIfItExists(id, trackChanges);
        return _mapper.Map<EmployeeDto>(employee); 
    }

    public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
    {
        await CheckIfItExists(companyId, trackChanges);

        var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

        _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
        await _repository.SaveAsync();

        var employeeToReturn = _mapper.Map<EmployeeDto>(employeeForCreation);
        return employeeToReturn;
    }

    public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid id, bool trackChanges) 
    {
        await CheckIfItExists(companyId, trackChanges);
        var employee = await GetEmployeeAndCheckIfItExists(id, trackChanges);

        _repository.Employee.DeleteEmployee(employee);
        await _repository.SaveAsync(); 
    }

    public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, 
        bool compTrackChanges, bool empTrackChanges) 
    {
        await CheckIfItExists(companyId, compTrackChanges);
        var employee = await GetEmployeeAndCheckIfItExists(id, empTrackChanges);

        _mapper.Map(employeeForUpdate, employee);
        await _repository.SaveAsync(); 
    }

    public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfItExists(companyId, compTrackChanges);
        var employee = await GetEmployeeAndCheckIfItExists(id, empTrackChanges);

        var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);
        return (employeeToPatch, employee);
    }

    public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity) 
    { 
        _mapper.Map(employeeToPatch, employeeEntity);
        await _repository.SaveAsync(); 
    }

    private async Task CheckIfItExists(Guid id, bool trackChanges)
    {
        var company = await _repository.Company.GetCompanyAsync(id, trackChanges);
        if (company is null)
            throw new CompanyNotFoundException(id);
    }

    private async Task<Employee> GetEmployeeAndCheckIfItExists(Guid id, bool trackChanges)
    {
        var employee = await _repository.Employee.GetEmployeeAsync(id, id, trackChanges);
        if (employee is null)
            throw new EmployeeNotFoundException(id);
        return employee;
    }
}
