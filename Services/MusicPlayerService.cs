using DSharpPlus.Lavalink;
using YumeChan.DreamJockey.Infrastructure;
using static YumeChan.DreamJockey.Infrastructure.OperationResults;

namespace YumeChan.DreamJockey.Services;

/// <summary>
/// Provides a service to manage a guild's music with basic playback controls.
/// </summary>
public sealed class MusicPlayerService
{
	private readonly MusicQueueService _queueService;
	private readonly Dictionary<ulong, LavalinkGuildConnection> _guildConnections = new();

	public MusicPlayerService(MusicQueueService queueService)
	{
		_queueService = queueService;
	}

	/// <summary>
	/// Connects to the voice channel specified in <see cref="vc"/>.
	/// </summary>
	public async Task<OperationResult> ConnectAsync(VoiceCommandContext vc)
	{
		if (_guildConnections.ContainsKey(vc.Channel.Guild.Id))
		{
			// Already connected
			return Failure();
		}

		await GetContextGuildConnectionAsync(vc, true);
		return Success();
	}


	/// <summary>
	/// Disconnects from the voice channel specified in <see cref="vc"/>.
	/// </summary>
	/// <returns>
	/// Returns a <see cref="bool"/> specifying wether a disconnection occured
	/// (false means there was no connection to begin with)
	/// </returns>
	public async Task<OperationResult> DisconnectAsync(VoiceCommandContext vc) => await GetContextGuildConnectionAsync(vc) is { Result: not null } connOp
			? await DisconnectAsync(connOp.Result)
			: Failure();

	/// <summary>
	/// Disconnects from the voice channel currently connected to, in guild with ID <see cref="guildId"/>.
	/// </summary>
	/// <returns>
	/// Returns a <see cref="bool"/> specifying wether a disconnection occured
	/// (false means there was no connection to begin with)
	/// </returns>
	public async Task<OperationResult> DisconnectAsync(ulong guildId) => _guildConnections.TryGetValue(guildId, out LavalinkGuildConnection? conn)
			? await DisconnectAsync(conn)
			: Failure();

	/// <summary>
	/// Internal method housing the disconnection logic.
	/// </summary>
	/// <param name="conn">Lavalink connection to disconnect</param>
	/// <returns></returns>
	private async Task<OperationResult> DisconnectAsync(LavalinkGuildConnection conn)
	{
		await conn.DisconnectAsync();
		_guildConnections.Remove(conn.Guild.Id);

		return Success();
	}

	/// <summary>
	/// Checks if DreamJockey is connected to a voice channel in the specified guild.
	/// </summary>
	public bool IsConnected(ulong guildId) => _guildConnections.ContainsKey(guildId);

	/// <summary>
	/// Plays a track found with the specified <see cref="query"/>.
	/// </summary>
	/// <remarks>
	///	Using this method will overwrite the current queue.
	/// </remarks>
	public async Task<OperationResult<IEnumerable<LavalinkTrack>>> PlayAsync(VoiceCommandContext vc, string query)
	{
		if ((await GetContextGuildConnectionAsync(vc, true)).Result is not { IsConnected: true } conn)
		{
			return Failure<IEnumerable<LavalinkTrack>>(null, "No connection to a voice channel.");
		}

		OperationResult<LavalinkLoadResult> loadResult = await LookupTracksAsync(vc, query);

		if (loadResult is not { Status: OperationStatus.Success, Result: { } tracks })
		{
			return Failure<IEnumerable<LavalinkTrack>>(loadResult.Result?.Tracks, loadResult.Message);
		}
		
		LavalinkTrack track = tracks.Tracks.First();
		_queueService.ClearMusicQueue(vc.Context.Guild.Id);
		_queueService.GetMusicQueue(vc, true); // Provision a queue if it doesn't exist.
		await conn.PlayAsync(track);

		return Success(tracks.Tracks, $"Now playing `{track.Title}`.");
	}

	/// <summary>
	/// Plays a track found with the specified <see cref="uri"/>.
	/// </summary>
	/// <remarks>
	///	Using this method will overwrite the current queue.
	/// </remarks>
	public async Task<OperationResult<LavalinkTrack>> PlayAsync(VoiceCommandContext vc, Uri uri)
	{
		if ((await GetContextGuildConnectionAsync(vc, true)).Result is not { IsConnected: true } conn)
        {
        	return Failure<LavalinkTrack>(null, "No connection to a voice channel.");
        }

		// Stop any current playback
		await conn.StopAsync();
		
		OperationResult<LavalinkLoadResult> loadResult = await LookupTracksAsync(vc, uri);

		if (loadResult is not { Status: OperationStatus.Success, Result: { } tracks })
		{
			return Failure<LavalinkTrack>(null, loadResult.Message);
		}

		LavalinkTrack track = tracks.Tracks.First();
		_queueService.ClearMusicQueue(vc.Context.Guild.Id);
		_queueService.GetMusicQueue(vc, true); // Provision a queue if it doesn't exist.
		await conn.PlayAsync(track);
		
		return Success(track, $"Now playing `{track.Title}`.");
	}
	
	/// <summary>
	/// Queues a track found with the specified <see cref="query"/>.
	/// </summary>
	/// <remarks>
	/// Using this method will play the track if there is no track currently playing.
	/// </remarks>
	public async Task<OperationResult<IEnumerable<LavalinkTrack>>> QueueAsync(VoiceCommandContext vc, string query)
	{
		// Get the current queue for the guild.
		// If there is no queue, forward the call to PlayAsync.
		if (_queueService.GetMusicQueue(vc) is not { Count: > 0 } queue)
		{
			return await PlayAsync(vc, query);
		}

		// Otherwise, lookup the track and add it to the queue.
		OperationResult<LavalinkLoadResult> loadResult = await LookupTracksAsync(vc, query);
		
		if (loadResult is not { Status: OperationStatus.Failure, Result: { } tracks })
		{
			return Failure<IEnumerable<LavalinkTrack>>(loadResult.Result?.Tracks, loadResult.Message);
		}
		
		LavalinkTrack track = tracks.Tracks.First();
		queue.Enqueue(track);
		
		return Success(tracks.Tracks, $"Queued `{track.Title}`.");
	}
	
	/// <summary>
	/// Queues a track found with the specified <see cref="uri"/>.
	/// </summary>
	/// <remarks>
	/// Using this method will play the track if there is no track currently playing.
	/// </remarks>
	public async Task<OperationResult<LavalinkTrack>> QueueAsync(VoiceCommandContext vc, Uri uri)
	{
		// Get the current queue for the guild.
		Queue<LavalinkTrack>? queue = _queueService.GetMusicQueue(vc);

		if (queue is not { Count: > 0 } && vc.GetGuildConnection()?.CurrentState.CurrentTrack is null)
		{
			return await PlayAsync(vc, uri);
		}

		// Otherwise, lookup the track and add it to the queue.
		LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(uri);

		if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
		{
			return Failure<LavalinkTrack>(null, $"Failed to find a track with URI `{uri}`.");
		}

		LavalinkTrack track = loadResult.Tracks.First();
		queue.Enqueue(track);
		
		return Success(track, $"Queued `{track.Title}`.");
	}

	/// <summary>
	/// Skips the current track.
	/// </summary>
	public async Task<OperationResult> SkipAsync(VoiceCommandContext vc)
	{
		if ((await GetContextGuildConnectionAsync(vc)).Result is not { } conn)
		{
			return Failure<LavalinkTrack>(null, "No connection to a voice channel.");
		}
		
		// Stop the current track.
		await conn.StopAsync();
		
		// Dequeue the next track (if it exists) and play it.
		// Return the result of the dequeue operation (Success for a dequeued track, Failure for no tracks present/left).
		if (_queueService.GetMusicQueue(vc) is { } queue)
		{
			// Try dequeue
			if (queue.TryDequeue(out LavalinkTrack? track))
			{
				await conn.PlayAsync(track);
				return Success($"Skipped to next track. Now playing `{track.Title}`.");
			}
		}
		
		// Nothing left to play (or no queue), return a warning.
		return new(OperationStatus.Warning, "No tracks left in the queue.");
	}
	
	/// <summary>
	/// Clears the current queue.
	/// </summary>
	/// <remarks>
	/// This method will not stop any track currrently playing.
	/// </remarks>
	public OperationResult ClearQueue(VoiceCommandContext vc)
	{
		// Check if there is a queue. Return a failure if there is not.
		if (_queueService.GetMusicQueue(vc) is not { Count: > 0 })
		{
			return new(OperationStatus.Failure, "No queue to clear.");
		}
		
		// Clear the queue.
		_queueService.ClearMusicQueue(vc.Context.Guild.Id);

		return Success("Queue cleared.");
	}	

	/// <summary>
	/// Stops the currently playing track.
	/// </summary>
	public async Task<OperationResult> StopAsync(VoiceCommandContext vc)
	{
		OperationResult<LavalinkGuildConnection> connResult = await GetContextGuildConnectionAsync(vc);

		static async Task<OperationResult> _StopAsync(LavalinkGuildConnection conn)
		{
			await conn.StopAsync();
			return Success("Player stopped.");
		}
		
		return connResult switch
		{
			{ Status: OperationStatus.Failure } => connResult,
			{ Result.CurrentState.CurrentTrack: null } => Failure("Sorry, there is nothing to pause."),
			{ Result: { IsConnected: true } conn } => await _StopAsync(conn),
			_ => throw new InvalidOperationException()
		};
	}

	/// <summary>
	/// Pauses the current track.
	/// </summary>
	public async Task<OperationResult> PauseAsync(VoiceCommandContext vc)
	{
		OperationResult<LavalinkGuildConnection> connResult = await GetContextGuildConnectionAsync(vc);

		static async Task<OperationResult> _PauseAsync(LavalinkGuildConnection conn)
		{
			await conn.PauseAsync();
			return Success($"Paused `{conn.CurrentState.CurrentTrack.Title}`.");
		}
		
		return connResult switch
		{
			{ Status: OperationStatus.Failure } => connResult,
			{ Result.CurrentState.CurrentTrack: null } => Failure("Sorry, there is nothing to pause."),
			{ Result: { } conn } => await _PauseAsync(conn),
			_ => throw new InvalidOperationException()
		};
	}

	/// <summary>
	/// Resumes the current track.
	/// </summary>
	public async Task<OperationResult> ResumeAsync(VoiceCommandContext vc)
	{
		OperationResult<LavalinkGuildConnection> connResult = await GetContextGuildConnectionAsync(vc);

		if (connResult is { Status: OperationStatus.Failure })
		{
			return connResult;
		}

		if (connResult.Result?.CurrentState.CurrentTrack is not { } track)
		{
			return Failure("Sorry, there is nothing to resume.");
		}

		await connResult.Result.ResumeAsync();

		return Success($"Resumed `{track.Title}`.");
	}

	/// <summary>
	/// Gets the lavalink connection associated to a guild for a given voice command context.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <param name="createIfNotExists">Whether to create a new connection if one doesn't exist.</param>
	private async Task<OperationResult<LavalinkGuildConnection>> GetContextGuildConnectionAsync(VoiceCommandContext vc, bool createIfNotExists = false)
	{
		// Get connection from dictionary
		if (_guildConnections.TryGetValue(vc.Channel.Guild.Id, out LavalinkGuildConnection? conn))
		{
			return conn;
		}
		
		// Otherwise grab from context
		if (vc.GetGuildConnection() is { IsConnected: true } existing)
		{
			// Add to dictionary for later use
			_guildConnections.TryAdd(vc.Channel.Guild.Id, existing);
			return existing;
		}

		// No connection. Can we create one?
		if (createIfNotExists)
		{
			LavalinkGuildConnection created = await vc.Node.ConnectAsync(vc.Channel);
			
			// Set bot's voice connection as deafened, for network traffic & privacy reasons
			await created.Guild.Members[vc.Context.Client.CurrentUser.Id].SetDeafAsync(true, "[DreamJockey] Set self to deafened for network traffic & privacy.");
			_guildConnections.Add(vc.Channel.Guild.Id, created);
			
			// Hook the disconnect event to remove the connection from the dictionary.
			created.DiscordWebSocketClosed += (_, _) =>
			{
				_guildConnections.Remove(vc.Channel.Guild.Id);
				return Task.CompletedTask;
			};
			
			return created;
		}

		// No connection and we can't create one.
		return Failure<LavalinkGuildConnection>(null, "Sorry, I'm currently not in any voice channel.");
	}

	/// <summary>
	/// Looks up and returns a track for a given search query.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <param name="query">The search query.</param>
	/// <returns>The track.</returns>
	private static async Task<OperationResult<LavalinkLoadResult>> LookupTracksAsync(VoiceCommandContext vc, string query)
	{
		LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(query);

		return loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches 
			? Failure<LavalinkLoadResult>(null, $"Failed to find tracks for query `{query}`.") 
			: loadResult;
	}

	/// <summary>
	/// Looks up and returns a track for a given URI.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <param name="uri">The URI.</param>
	/// <returns>The track.</returns>
	private static async Task<OperationResult<LavalinkLoadResult>> LookupTracksAsync(VoiceCommandContext vc, Uri uri)
	{
		LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(uri);

		return loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches 
			? Failure<LavalinkLoadResult>(null, $"Failed to find tracks for URI `{uri}`.") 
			: loadResult;
	}
}