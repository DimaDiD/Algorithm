﻿using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AlgorithmDataContext _dataContext;

    public UnitOfWork(AlgorithmDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) =>
        _dataContext.Database.BeginTransactionAsync(isolationLevel);
}
