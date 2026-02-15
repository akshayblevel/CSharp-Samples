```c#
MultiTenant.EFCore.Nuget
	Common
		namespace MultiTenant.EFCore.Nuget.Common
		{
			[AttributeUsage(AttributeTargets.Class)]
			public sealed class DatabaseBoundAttribute : Attribute
			{
				public DatabaseKey Key { get; }
				public DatabaseBoundAttribute(DatabaseKey key) => Key = key;
			}
		}
		
		namespace MultiTenant.EFCore.Nuget.Common
		{
			public enum DatabaseKey
			{
				Tenant,
				SharedAuth,
				SharedConfig
			}
		}
		
		namespace MultiTenant.EFCore.Nuget.Common
		{
			public static class DateHelper
			{
				public static DateTime GetCurrentDateTime()
				{
					DateTime now = DateTime.Now;
					return now;
				}

				public static string GetCurrentFormattedDateTime(string format = "MM-dd-yyyy HH:mm:ss")
				{
					DateTime now = GetCurrentDateTime();
					return now.ToString(format);
				}
			}

		}
		
		namespace MultiTenant.EFCore.Nuget.Common
		{
			public static class OutboxExtensions
			{
				public static void AddOutboxMessage(
					this DbContext context,
					object @event)
				{
					var message = new OutboxMessage
					{
						Id = Guid.NewGuid(),
						Type = @event.GetType().Name,
						Payload = JsonSerializer.Serialize(@event),
						OccurredOnUtc = DateTime.UtcNow
					};

					context.Set<OutboxMessage>().Add(message);
				}
			}
		}
		
	Interfaces
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface IDbContextProvider
			{
				IReadOnlyDictionary<DatabaseKey, DbContext> GetDbContexts();
			}
		}
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface IDbContextResolver
			{
				DbContext Resolve(DatabaseKey key);
			}
		}

		using Microsoft.Data.SqlClient;
		using Microsoft.EntityFrameworkCore;
		using System.Linq.Expressions;

		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface IReadOnlyRepository<T> where T : class
			{
				DbSet<T> DbSet();
				Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> where);
				Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> where, IEnumerable<string> includes);
				Task<List<T>> ToListAsync(Expression<Func<T, bool>> where);
				Task<List<T>> ToListAsync(Expression<Func<T, bool>> where, IEnumerable<string> includes);
				Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> where);
				Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> where, IEnumerable<string> includes);
				Task<IEnumerable<T>> GetAsyncWithQuery(Expression<Func<IQueryable<T>, IQueryable<T>>> conditions);
				Task<List<T>> QueryProcedure(string procedureName, params SqlParameter[] sqlParameters);
				Task<bool> AnyAsync(Expression<Func<T, bool>> where);
			}

		}
		
		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface IRepository<T> :
			IReadOnlyRepository<T>,
			IWriteOnlyRepository<T>
			where T : class
			{
			}
		}
		
		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface ITenantContext
			{
				string? TenantId { get; }
				string? TenantConnectionString { get; }
			}
		}
		
		using Microsoft.Data.SqlClient;
		using Microsoft.EntityFrameworkCore.Storage;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface IUnitOfWork
			{
				IRepository<T> Repository<T>(DatabaseKey databaseKey)
					where T : class;

				Task<int> CompleteAsync(DatabaseKey databaseKey);

				Task ExecuteTransactionAsync(DatabaseKey databaseKey, Func<IDbContextTransaction, Task> operation);
				Task<bool> ExecuteAutoTransactionAsync(DatabaseKey databaseKey, Func<IDbContextTransaction, Task> operations);
				Task<(List<T1>, List<T2>)> ExecuteMultipleResultSetsAsync<T1, T2>(DatabaseKey databaseKey, string storedProcedure, params SqlParameter[] parameters) where T1 : class, new() where T2 : class, new();

			}
		}
		
		using System.Linq.Expressions;

		namespace MultiTenant.EFCore.Nuget.Interfaces
		{
			public interface IWriteOnlyRepository<T> where T : class
			{
				Task<T> CreateAsync(T entity);
				Task<bool> SaveAsync(bool acceptAllChanges = true);
				Task<T> UpdateAsync(T entity);
				Task UpdateRangeAsync(IEnumerable<T> entities);
				Task DeleteAsync(T entity);
				Task BulkUpsertAsync(List<T> entities, Expression<Func<T, object>> uniquePropertyExpression, Expression<Func<T, object>>[] updateProperties);
			}

		}
		
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;
		using MultiTenant.EFCore.Nuget.Interfaces;

		namespace MultiTenant.EFCore.Nuget
		{
			public sealed class DbContextResolver : IDbContextResolver
			{
				private readonly IReadOnlyDictionary<DatabaseKey, DbContext> _contexts;

				public DbContextResolver(IDbContextProvider provider)
				{
					_contexts = provider.GetDbContexts();
				}

				public DbContext Resolve(DatabaseKey key)
					=> _contexts.TryGetValue(key, out var db)
						? db
						: throw new InvalidOperationException(
							$"No DbContext registered for database '{key}'");
			}
		}

		namespace MultiTenant.EFCore.Nuget
		{
			public sealed class OutboxMessage
			{
				public Guid Id { get; set; }
				public string Type { get; set; } = default!;
				public string Payload { get; set; } = default!;
				public DateTime OccurredOnUtc { get; set; }
				public DateTime? ProcessedOnUtc { get; set; }
			}
		}
		
		using Microsoft.Data.SqlClient;
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;
		using MultiTenant.EFCore.Nuget.Interfaces;
		using System.Linq.Expressions;
		using System.Reflection;

		namespace MultiTenant.EFCore.Nuget
		{
			public sealed class Repository<T> : RepositoryBase<T>,IRepository<T> where T : class
			{
				public Repository(DbContext context,DatabaseKey databaseKey) : base(context, databaseKey)
				{
					var attr = typeof(T).GetCustomAttribute<DatabaseBoundAttribute>();
					if (attr != null && attr.Key != databaseKey)
					{
						throw new InvalidOperationException(
							$"{typeof(T).Name} cannot be used with {databaseKey}");
					}
				}
				public DbContext DataContext { get; set; }

				public DbSet<T> DbSet()
				{
					DataContext.Database.SetCommandTimeout(180);
					return DataContext.Set<T>();
				}


				public async Task<T> CreateAsync(T entity)
			{
				var newEntity = await DataContext.Set<T>().AddAsync(entity);
				return newEntity.Entity;
			}

			public async Task DeleteAsync(T entity)
			{
				DataContext.Set<T>().Remove(entity);
			}

			public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> @where, IEnumerable<string> includes)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				if (includes != null)
				{
					queryable = includes.Aggregate(queryable, (current, includes) => current.Include(includes));
				}
				var entity = await queryable.Where(where).FirstOrDefaultAsync();
				return entity;
			}

			public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> where)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				var entity = await queryable.Where(where).FirstOrDefaultAsync();
				return entity;
			}

			public async Task<bool> AnyAsync(Expression<Func<T, bool>> where)
			{
				var queryable = DataContext.Set<T>().AsNoTracking();
				var entity = await queryable.Where(where).AnyAsync();
				return entity;
			}

			public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> where, IEnumerable<string> includes)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				if (includes != null)
				{
					queryable = includes.Aggregate(queryable, (current, includes) => current.Include(includes));
				}
				return await queryable.Where(where).ToListAsync();
			}

			public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> where)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				return await queryable.Where(where).ToListAsync();
			}

			public async Task<IEnumerable<T>> GetAsyncWithQuery(Expression<Func<IQueryable<T>, IQueryable<T>>> conditions)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				var compiledConditions = conditions.Compile();
				queryable = compiledConditions(queryable);
				return await queryable.ToListAsync();
			}

			public async Task<bool> SaveAsync(bool acceptAllChanges = true)
			{
				var currentUser = DataContext.GetType().GetProperty("UserName")?.GetValue(DataContext, null) as string;
				var currentDate = DateHelper.GetCurrentDateTime();

				foreach (var entry in DataContext.ChangeTracker.Entries())
				{
					if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
					{
						var entity = entry.Entity;

						var createdByProperty = entity.GetType().GetProperty("CreatedBy");
						var updatedByProperty = entity.GetType().GetProperty("LastUpdatedBy");
						var createdDateProperty = entity.GetType().GetProperty("CreatedOn");
						var updatedDateProperty = entity.GetType().GetProperty("LastUpdatedOn");

						if (createdByProperty != null && createdDateProperty != null)
						{
							if (entry.State == EntityState.Added)
							{
								createdByProperty.SetValue(entity, currentUser);
								createdDateProperty.SetValue(entity, currentDate);
							}
						}

						if (updatedByProperty != null && updatedDateProperty != null)
						{
							if (entry.State == EntityState.Modified)
							{
								updatedByProperty.SetValue(entity, currentUser);
								updatedDateProperty.SetValue(entity, currentDate);
							}
						}
					}
				}
				return await DataContext.SaveChangesAsync(acceptAllChanges) > 0;
			}

			public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> where, IEnumerable<string> includes)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				if (includes != null)
				{
					queryable = includes.Aggregate(queryable, (current, includes) => current.Include(includes));
				}
				var entities = await queryable.Where(where).ToListAsync();
				return entities;
			}

			public async Task<List<T>> ToListAsync(Expression<Func<T, bool>> where)
			{
				var queryable = DataContext.Set<T>().AsQueryable().AsNoTracking().AsSplitQuery();
				var entities = await queryable.Where(where).ToListAsync();
				return entities;
			}

			public async Task<T> UpdateAsync(T entity)
			{
				var entry = DataContext.Set<T>().Update(entity);
				return entry.Entity;
			}

			public Task UpdateRangeAsync(IEnumerable<T> entities)
			{
				DataContext.Set<T>().UpdateRange(entities);
				return SaveAsync();
			}

			public async Task<List<T>> QueryProcedure(string procedureName, params SqlParameter[] sqlParameters)
			{
				var query = $"EXEC {procedureName} {string.Join(", ", sqlParameters.Select(s => $"@{s.ParameterName}"))}";

				Console.WriteLine($"Generated SQL query: {query}");

				return await DataContext.Set<T>()
					.FromSqlRaw(query, sqlParameters)
					.AsNoTracking().AsSplitQuery()
					.ToListAsync();
			}

			public async Task BulkUpsertAsync(List<T> entities,Expression<Func<T, object>> uniquePropertyExpression,Expression<Func<T, object>>[] updateProperties)
			{
				const int batchSize = 100;
				var dbSet = DataContext.Set<T>();

				var uniqueProperty = (PropertyInfo)((MemberExpression)uniquePropertyExpression.Body).Member;
				var updatePropertyInfos = updateProperties.Select(p => (PropertyInfo)((MemberExpression)p.Body).Member).ToList();

				var uniqueValues = entities.Select(e => uniqueProperty.GetValue(e)).ToList();

				for (int i = 0; i < entities.Count; i += batchSize)
				{
					var batch = entities.Skip(i).Take(batchSize).ToList();

					var existingEntities = dbSet
						.AsEnumerable() 
						.Where(e => uniqueValues.Contains(uniqueProperty.GetValue(e)))
						.ToList();

					foreach (var entity in batch)
					{
						var uniqueValue = uniqueProperty.GetValue(entity);
						var existingEntity = existingEntities
							.FirstOrDefault(e => uniqueProperty.GetValue(e).Equals(uniqueValue));

						if (existingEntity != null)
						{
							// Update the entity's properties dynamically
							foreach (var propertyInfo in updatePropertyInfos)
							{
								var newValue = propertyInfo.GetValue(entity);
								propertyInfo.SetValue(existingEntity, newValue);
							}
						}
						else
						{
							// Insert the new entity
							await dbSet.AddAsync(entity);
						}
					}

					await DataContext.SaveChangesAsync();
				}
			}

			}
		}
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.Nuget
		{
			public abstract class RepositoryBase<T> where T : class
			{
				protected DbContext Context { get; }
				protected DatabaseKey DatabaseKey { get; }
				protected DbSet<T> Set => Context.Set<T>();

				protected RepositoryBase(DbContext context, DatabaseKey databaseKey)
				{
					Context = context;
					DatabaseKey = databaseKey;
				}
			}
		}
		
		using Microsoft.Data.SqlClient;
		using Microsoft.EntityFrameworkCore;
		using Microsoft.EntityFrameworkCore.Metadata.Internal;
		using Microsoft.EntityFrameworkCore.Storage;
		using Microsoft.Extensions.DependencyInjection;
		using MultiTenant.EFCore.Nuget.Common;
		using MultiTenant.EFCore.Nuget.Interfaces;
		using System.Data;
		using System.Data.Common;
		using System.Reflection;

		namespace MultiTenant.EFCore.Nuget
		{
			public sealed class UnitOfWork : IUnitOfWork 
			{
				private readonly IDbContextResolver _resolver;
				private readonly Dictionary<(Type, DatabaseKey), object> _repositories = new();
				private readonly IServiceProvider _serviceProvider;
				public UnitOfWork(IDbContextResolver resolver, IServiceProvider serviceProvider)
				{
					_resolver = resolver;
					_serviceProvider = serviceProvider;
				}

				public IRepository<T> Repository<T>(DatabaseKey key) where T : class
				{
					var cacheKey = (typeof(T), key);

					if (_repositories.TryGetValue(cacheKey, out var repo))
						return (IRepository<T>)repo;

					var context = _resolver.Resolve(key);
					var repository = _serviceProvider.GetRequiredService<IRepository<T>>(); ;

					_repositories[cacheKey] = repository;
					return repository;
				}

				public Task<int> CompleteAsync(DatabaseKey key)
					=> _resolver.Resolve(key).SaveChangesAsync();

				public async Task ExecuteTransactionAsync(DatabaseKey key,Func<IDbContextTransaction, Task> operations)
				{
					const int maxRetryCount = 3;
					TimeSpan delay = TimeSpan.FromSeconds(2);

					for (int attempt = 1; attempt <= maxRetryCount; attempt++)
					{

						var context = _resolver.Resolve(key);

						await using var transaction = await context.Database.BeginTransactionAsync();

						try
						{
							await operations(transaction);
							await context.SaveChangesAsync();
							await transaction.CommitAsync();
							return; // Success
						}
						catch (Exception ex) when (IsTransient(ex) && attempt < maxRetryCount)
						{
							await transaction.RollbackAsync();
							await Task.Delay(delay);
							delay *= 2; // Exponential backoff
						}
						catch
						{
							await transaction.RollbackAsync();
						}

					}
				}

				public async Task<bool> ExecuteAutoTransactionAsync(DatabaseKey key, Func<IDbContextTransaction, Task> operations)
				{
					var context = _resolver.Resolve(key);
					var strategy = context.Database.CreateExecutionStrategy();
					bool result = false;
					await strategy.ExecuteAsync(async () =>
					{
						await using var transaction = await context.Database.BeginTransactionAsync();
						try
						{
							await operations(transaction);
							await SaveAuditPropertiesAsync(key);
							await context.SaveChangesAsync();
							await transaction.CommitAsync();
							result = true;
						}
						catch
						{
							await transaction.RollbackAsync();
						}
					});
					return result;
				}

				private static bool IsTransient(Exception ex)
				{
					if (ex is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException is SqlException sqlEx)
					{
						return sqlEx.Number == 1205 || // Deadlock
							   sqlEx.Number == -2;     // Timeout
					}

					if (ex is SqlException sqlExDirect)
					{
						return sqlExDirect.Number == 1205 || sqlExDirect.Number == -2;
					}

					return false;
				}

				private async Task SaveAuditPropertiesAsync(DatabaseKey key)
				{
					var context = _resolver.Resolve(key);
					var currentUser = context.GetType().GetProperty("UserName")?.GetValue(context, null) as string;
					var currentDate = DateHelper.GetCurrentDateTime();

					foreach (var entry in context.ChangeTracker.Entries())
					{
						if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
						{
							var entity = entry.Entity;

							var createdByProperty = entity.GetType().GetProperty("CreatedBy");
							var updatedByProperty = entity.GetType().GetProperty("LastUpdatedBy");
							var createdDateProperty = entity.GetType().GetProperty("CreatedOn");
							var updatedDateProperty = entity.GetType().GetProperty("LastUpdatedOn");

							if (createdByProperty != null && createdDateProperty != null)
							{
								if (entry.State == EntityState.Added)
								{
									createdByProperty.SetValue(entity, currentUser);
									createdDateProperty.SetValue(entity, currentDate);
								}
							}

							if (updatedByProperty != null && updatedDateProperty != null)
							{
								if (entry.State == EntityState.Modified)
								{
									updatedByProperty.SetValue(entity, currentUser);
									updatedDateProperty.SetValue(entity, currentDate);
								}
							}
						}
					}
				}

				public async Task<(List<T1>, List<T2>)> ExecuteMultipleResultSetsAsync<T1, T2>(DatabaseKey key, string storedProcedure, params SqlParameter[] parameters) where T1 : class, new() where T2 : class, new()
				{
					var context = _resolver.Resolve(key);
					using var connection = context.Database.GetDbConnection();
					using var command = connection.CreateCommand();
					command.CommandText = storedProcedure;
					command.CommandType = CommandType.StoredProcedure;

					if (parameters?.Length > 0)
						command.Parameters.AddRange(parameters);

					if (connection.State != ConnectionState.Open)
						await connection.OpenAsync();

					using var reader = await command.ExecuteReaderAsync();

					var result1 = await MapResultSet<T1>(reader);

					await reader.NextResultAsync();

					var result2 = await MapResultSet<T2>(reader);

					return (result1, result2);
				}

				private async Task<List<T>> MapResultSet<T>(DbDataReader reader) where T : class, new()
				{
					var entities = new List<T>();
					var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

					var columnNames = Enumerable.Range(0, reader.FieldCount)
						.Select(reader.GetName)
						.ToHashSet(StringComparer.OrdinalIgnoreCase);

					while (await reader.ReadAsync())
					{
						var entity = new T();
						foreach (var prop in properties)
						{
							if (!prop.CanWrite || !columnNames.Contains(prop.Name))
								continue;

							var value = reader[prop.Name];
							if (value != DBNull.Value)
								prop.SetValue(entity, value);
						}
						entities.Add(entity);
					}

					return entities;
				}

			}
		}

		------------------------------------------------------------------
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.App.DB
		{
			public sealed class SharedAuthDbContext : DbContext
			{
				public DbSet<UserCredential> UserCredentials => Set<UserCredential>();
			}

			[DatabaseBound(DatabaseKey.SharedAuth)]
			public class UserCredential
			{
			}
		}
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.App.DB
		{
			public sealed class SharedConfigDbContext : DbContext
			{
				public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
			}

			[DatabaseBound(DatabaseKey.SharedConfig)]
			public class SystemSetting
			{
			}
		}
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.Nuget.Common;

		namespace MultiTenant.EFCore.App.DB
		{
			public sealed class TenantDbContext : DbContext
			{
				public DbSet<Order> Orders => Set<Order>();
			}

			[DatabaseBound(DatabaseKey.Tenant)]
			public class Order
			{
			}
		}
		
		using Microsoft.EntityFrameworkCore;
		using MultiTenant.EFCore.App.DB;
		using MultiTenant.EFCore.Nuget.Common;
		using MultiTenant.EFCore.Nuget.Interfaces;

		namespace MultiTenant.EFCore.App
		{
			public sealed class AppDbContextProvider : IDbContextProvider
			{
				private readonly TenantDbContext _tenantDb;
				private readonly SharedAuthDbContext _authDb;
				private readonly SharedConfigDbContext _configDb;

				public AppDbContextProvider(
					TenantDbContext tenantDb,
					SharedAuthDbContext authDb,
					SharedConfigDbContext configDb)
				{
					_tenantDb = tenantDb;
					_authDb = authDb;
					_configDb = configDb;
				}

				public IReadOnlyDictionary<DatabaseKey, DbContext> GetDbContexts()
					=> new Dictionary<DatabaseKey, DbContext>
					{
						[DatabaseKey.Tenant] = _tenantDb,
						[DatabaseKey.SharedAuth] = _authDb,
						[DatabaseKey.SharedConfig] = _configDb
					};
			}
		}
		
		
		builder.Services.AddDbContext<TenantDbContext>(options =>
		options.UseSqlServer("tenantConnectionString"));

		builder.Services.AddDbContext<SharedAuthDbContext>(options =>
			options.UseSqlServer("sharedAuthConnection"));

		builder.Services.AddDbContext<SharedConfigDbContext>(options =>
			options.UseSqlServer("sharedConfigConnection"));

		builder.Services.AddScoped<IDbContextProvider, AppDbContextProvider>();
		builder.Services.AddScoped<IDbContextResolver, DbContextResolver>();
		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();






```
