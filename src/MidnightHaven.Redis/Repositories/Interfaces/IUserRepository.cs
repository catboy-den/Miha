﻿using MidnightHaven.Redis.Documents;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IUserRepository : IDocumentRepository<UserDocument>
{
    Task<IEnumerable<UserDocument>> GetAllUsersWithBirthdayEnabledAsync();
}
