using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.Repository.DbModels;
using Thinktecture.Relay.Server.Security;

namespace Thinktecture.Relay.Server.Repository
{
	public class LinkRepository : ILinkRepository
	{
		private static readonly Dictionary<string, PasswordInformation> _successfullyValidatedUsernamesAndPasswords = new Dictionary<string, PasswordInformation>();

		private readonly ILogger _logger;
		private readonly IPasswordHash _passwordHash;
		private readonly IConfiguration _configuration;

		private DateTime ActiveLinkTimeout => DateTime.UtcNow - _configuration.ActiveConnectionTimeout;

		public LinkRepository(ILogger logger, IPasswordHash passwordHash, IConfiguration configuration)
		{
			_logger = logger;
			_passwordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public PageResult<LinkDetails> GetLinkDetails(PageRequest paging)
		{
			using (var context = new RelayContext())
			{
				var linksQuery = context.Links.AsQueryable();

				if (!String.IsNullOrWhiteSpace(paging.SearchText))
				{
					var searchText = paging.SearchText.ToLower();
					linksQuery = linksQuery.Where(w => w.UserName.Contains(searchText) || w.SymbolicName.Contains(searchText));
				}

				// Default sorting must be provided
				if (String.IsNullOrWhiteSpace(paging.SortField))
				{
					paging.SortField = "SymbolicName";
					paging.SortDirection = SortDirection.Asc;
				}

				var numberOfLinks = linksQuery.Count();

				linksQuery = linksQuery.OrderByPropertyName(paging.SortField, paging.SortDirection);
				linksQuery = linksQuery.ApplyPaging(paging);

				return new PageResult<LinkDetails>()
				{
					Items = GetLinkDetailsFromDbLink(linksQuery).ToList(),
					Count = numberOfLinks,
				};
			}
		}

		public Link GetLink(Guid linkId)
		{
			using (var context = new RelayContext())
			{
				var linkQuery = context.Links
					.Where(l => l.Id == linkId);

				return GetLinkFromDbLink(linkQuery).FirstOrDefault();
			}
		}

		public LinkDetails GetLinkDetails(Guid linkId)
		{
			using (var context = new RelayContext())
			{
				var linkQuery = context.Links
					.Where(l => l.Id == linkId);

				return GetLinkDetailsFromDbLink(linkQuery).FirstOrDefault();
			}
		}

		public Link GetLink(string linkName)
		{
			using (var context = new RelayContext())
			{
				var linkQuery = context.Links
					.Where(p => p.UserName == linkName);

				return GetLinkFromDbLink(linkQuery).FirstOrDefault();
			}
		}

		public LinkConfiguration GetLinkConfiguration(Guid linkId)
		{
			using (var context = new RelayContext())
			{
				var linkQuery = context.Links
					.Where(l => l.Id == linkId);

				return GetLinkConfigurationFromDbLink(linkQuery).FirstOrDefault();
			}
		}

		private IQueryable<Link> GetLinkFromDbLink(IQueryable<DbLink> linksQuery)
		{
			return linksQuery
				.Select(l => new
				{
					link = l,
					ActiveConnections = l.ActiveConnections
						.Where(ac => ac.ConnectorVersion == 0 || ac.LastActivity > ActiveLinkTimeout)
						.Select(ac => ac.ConnectionId)
				})
				.Select(i => new Link()
				{
					Id = i.link.Id,
					ForwardOnPremiseTargetErrorResponse = i.link.ForwardOnPremiseTargetErrorResponse,
					IsDisabled = i.link.IsDisabled,
					SymbolicName = i.link.SymbolicName,
					AllowLocalClientRequestsOnly = i.link.AllowLocalClientRequestsOnly,
				});
		}

		private IQueryable<LinkDetails> GetLinkDetailsFromDbLink(IQueryable<DbLink> linksQuery)
		{
			return linksQuery
				.Select(link => new
				{
					link,
					link.ActiveConnections
				})
				.ToList()
				.Select(i => new LinkDetails()
				{
					Id = i.link.Id,
					CreationDate = i.link.CreationDate,
					ForwardOnPremiseTargetErrorResponse = i.link.ForwardOnPremiseTargetErrorResponse,
					IsDisabled = i.link.IsDisabled,
					MaximumLinks = i.link.MaximumLinks,
					SymbolicName = i.link.SymbolicName,
					UserName = i.link.UserName,
					AllowLocalClientRequestsOnly = i.link.AllowLocalClientRequestsOnly,

					TokenRefreshWindow = i.link.TokenRefreshWindow,
					HeartbeatInterval = i.link.HeartbeatInterval,
					ReconnectMinWaitTime = i.link.ReconnectMinWaitTime,
					ReconnectMaxWaitTime = i.link.ReconnectMaxWaitTime,
					AbsoluteConnectionLifetime = i.link.AbsoluteConnectionLifetime,
					SlidingConnectionLifetime = i.link.SlidingConnectionLifetime,

					Connections = i.ActiveConnections
						.Select(ac => ac.ConnectionId
							+ "; Versions: Connector = " + ac.ConnectorVersion + ", Assembly = " + ac.AssemblyVersion
							+ "; Last Activity: " + ac.LastActivity.ToString("yyyy-MM-dd hh:mm:ss")
							+ ((ac.LastActivity + _configuration.ActiveConnectionTimeout <= DateTime.UtcNow) ? " (inactive)" : "")
						)
						.ToList(),
				})
				.AsQueryable();
		}

		private IQueryable<LinkConfiguration> GetLinkConfigurationFromDbLink(IQueryable<DbLink> linksQuery)
		{
			return linksQuery
				.Select(link => new
				{
					link,
				})
				.ToList()
				.Select(i => new LinkConfiguration()
				{
					TokenRefreshWindow = i.link.TokenRefreshWindow,
					HeartbeatInterval = i.link.HeartbeatInterval,
					ReconnectMinWaitTime = i.link.ReconnectMinWaitTime,
					ReconnectMaxWaitTime = i.link.ReconnectMaxWaitTime,
					AbsoluteConnectionLifetime = i.link.AbsoluteConnectionLifetime,
					SlidingConnectionLifetime = i.link.SlidingConnectionLifetime,
				})
				.AsQueryable();
		}

		public CreateLinkResult CreateLink(string symbolicName, string userName)
		{
			using (var context = new RelayContext())
			{
				var password = _passwordHash.GeneratePassword(_configuration.LinkPasswordLength);
				var passwordInformation = _passwordHash.CreatePasswordInformation(password);

				var link = new DbLink()
				{
					Id = Guid.NewGuid(),
					Password = passwordInformation.Hash,
					Salt = passwordInformation.Salt,
					Iterations = passwordInformation.Iterations,
					SymbolicName = symbolicName,
					UserName = userName,
					CreationDate = DateTime.UtcNow,
				};

				context.Links.Add(link);
				context.SaveChanges();

				var result = new CreateLinkResult()
				{
					Id = link.Id,
					Password = Convert.ToBase64String(password),
				};

				return result;
			}
		}

		public bool UpdateLink(LinkDetails link)
		{
			using (var context = new RelayContext())
			{
				var linkEntity = context.Links.SingleOrDefault(p => p.Id == link.Id);

				if (linkEntity == null)
					return false;

				linkEntity.CreationDate = link.CreationDate;
				linkEntity.AllowLocalClientRequestsOnly = link.AllowLocalClientRequestsOnly;
				linkEntity.ForwardOnPremiseTargetErrorResponse = link.ForwardOnPremiseTargetErrorResponse;
				linkEntity.IsDisabled = link.IsDisabled;
				linkEntity.MaximumLinks = link.MaximumLinks;
				linkEntity.SymbolicName = link.SymbolicName;
				linkEntity.UserName = link.UserName;

				linkEntity.TokenRefreshWindow = link.TokenRefreshWindow;
				linkEntity.HeartbeatInterval = link.HeartbeatInterval;
				linkEntity.ReconnectMinWaitTime = link.ReconnectMinWaitTime;
				linkEntity.ReconnectMaxWaitTime = link.ReconnectMaxWaitTime;
				linkEntity.AbsoluteConnectionLifetime = link.AbsoluteConnectionLifetime;
				linkEntity.SlidingConnectionLifetime = link.SlidingConnectionLifetime;

				context.Entry(linkEntity).State = EntityState.Modified;

				return context.SaveChanges() == 1;
			}
		}

		public void DeleteLink(Guid linkId)
		{
			using (var context = new RelayContext())
			{
				var itemToDelete = new DbLink
				{
					Id = linkId,
				};

				context.Links.Attach(itemToDelete);
				context.Links.Remove(itemToDelete);

				context.SaveChanges();
			}
		}

		public IEnumerable<LinkDetails> GetLinkDetails()
		{
			using (var context = new RelayContext())
			{
				return GetLinkDetailsFromDbLink(context.Links).ToList();
			}
		}

		public bool Authenticate(string userName, string password, out Guid linkId)
		{
			linkId = Guid.Empty;

			using (var context = new RelayContext())
			{
				byte[] passwordBytes;

				try
				{
					passwordBytes = Convert.FromBase64String(password);
				}
				catch
				{
					return false;
				}

				var link = context.Links.Where(p => p.UserName == userName).Select(p => new
				{
					p.Id,
					p.Password,
					p.Iterations,
					p.Salt,
				}).FirstOrDefault();

				if (link == null)
				{
					return false;
				}

				var passwordInformation = new PasswordInformation()
				{
					Hash = link.Password,
					Iterations = link.Iterations,
					Salt = link.Salt,
				};

				var cacheKey = userName + "/" + password;
				PasswordInformation previousInfo;

				lock (_successfullyValidatedUsernamesAndPasswords)
				{
					_successfullyValidatedUsernamesAndPasswords.TryGetValue(cacheKey, out previousInfo);
				}

				// found in cache (NOTE: cache only contains successfully validated passwords to prevent DOS attacks!)
				if (previousInfo != null)
				{
					if (previousInfo.Hash == passwordInformation.Hash
						&& previousInfo.Iterations == passwordInformation.Iterations
						&& previousInfo.Salt == passwordInformation.Salt)
					{
						linkId = link.Id;
						return true;
					}
				}

				// ELSE: calculate and cache
				if (!_passwordHash.ValidatePassword(passwordBytes, passwordInformation))
				{
					return false;
				}

				lock (_successfullyValidatedUsernamesAndPasswords)
				{
					{
						_successfullyValidatedUsernamesAndPasswords[cacheKey] = passwordInformation;
					}
				}

				linkId = link.Id;
				return true;
			}
		}

		public bool IsUserNameAvailable(string userName)
		{
			using (var context = new RelayContext())
			{
				return !context.Links.Any(p => p.UserName == userName);
			}
		}

		public async Task AddOrRenewActiveConnectionAsync(Guid linkId, Guid originId, string connectionId, int connectorVersion, string assemblyVersion)
		{
			_logger?.Verbose("Adding or updating connection. connection-id={ConnectionId}, link-id={LinkId}, connector-version={ConnectorVersion}, connector-assembly-version={ConnectorAssemblyVersion}", connectionId, linkId, connectorVersion, assemblyVersion);

			try
			{
				using (var context = new RelayContext())
				{
					var activeConnection = await context.ActiveConnections
						.FirstOrDefaultAsync(ac => ac.LinkId == linkId && ac.OriginId == originId && ac.ConnectionId == connectionId).ConfigureAwait(false);

					if (activeConnection != null)
					{
						activeConnection.LastActivity = DateTime.UtcNow;
						activeConnection.ConnectorVersion = connectorVersion;
						activeConnection.AssemblyVersion = assemblyVersion;

						context.Entry(activeConnection).State = EntityState.Modified;
					}
					else
					{
						context.ActiveConnections.Add(new DbActiveConnection()
						{
							LinkId = linkId,
							OriginId = originId,
							ConnectionId = connectionId,
							ConnectorVersion = connectorVersion,
							AssemblyVersion = assemblyVersion,
							LastActivity = DateTime.UtcNow,
						});
					}

					await context.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during adding or renewing an active connection. link-id={LinkId}, connection-id={ConnectionId}, connector-version={ConnectorVersion}, connector-assembly-version={ConnectorAssemblyVersion}", linkId, connectionId, connectorVersion, assemblyVersion);
			}
		}

		public async Task RenewActiveConnectionAsync(string connectionId)
		{
			_logger?.Verbose("Renewing last activity. connection-id={ConnectionId}", connectionId);

			try
			{
				using (var context = new RelayContext())
				{
					var activeConnection = await context.ActiveConnections.FirstOrDefaultAsync(ac => ac.ConnectionId == connectionId).ConfigureAwait(false);

					if (activeConnection != null)
					{
						activeConnection.LastActivity = DateTime.UtcNow;

						context.Entry(activeConnection).State = EntityState.Modified;

						await context.SaveChangesAsync().ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during renewing an active connection. connection-id={ConnectionId}", connectionId);
			}
		}

		public async Task RemoveActiveConnectionAsync(string connectionId)
		{
			_logger?.Verbose("Deleting active connection. connection-id={ConnectionId}", connectionId);

			try
			{
				using (var context = new RelayContext())
				{
					var activeConnection = await context.ActiveConnections.FirstOrDefaultAsync(ac => ac.ConnectionId == connectionId).ConfigureAwait(false);

					if (activeConnection != null)
					{
						context.ActiveConnections.Remove(activeConnection);

						await context.SaveChangesAsync().ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during removing an active connection. connection-id={ConnectionId}", connectionId);
			}
		}

		public void DeleteAllConnectionsForOrigin(Guid originId)
		{
			_logger?.Verbose("Deleting all active connections");

			try
			{
				using (var context = new RelayContext())
				{
					var invalidConnections = context.ActiveConnections.Where(ac => ac.OriginId == originId).ToList();
					context.ActiveConnections.RemoveRange(invalidConnections);

					context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during deleting of all active connections");
			}
		}
	}
}
