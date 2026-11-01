﻿using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MMSA.DAL.Repositories.Interfaces;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}
