using DSharpPlus.Lavalink;
using YumeChan.DreamJockey.Infrastructure;
using static YumeChan.DreamJockey.Infrastructure.OperationResults;

namespace YumeChan.DreamJockey.Services;

/// <summary>
/// Provides a service to manage a guild's music with basic playback controls.
/// </summary>
public class MusicPlayerService
{
	private readonly Dictionary<ulong, LavalinkGuildConnection> _guildConnections = new();

	public MusicPlayerService() { }

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

		await ConnectInternalAsync(vc);

		return Success();
	}

	internal async Task<LavalinkGuildConnection> ConnectInternalAsync(VoiceCommandContext vc)
	{
		LavalinkGuildConnection conn = await vc.Node.ConnectAsync(vc.Channel);
		_guildConnections.Add(vc.Channel.Guild.Id, conn);

		return conn;
	}


	/// <summary>
	/// Disconnects from the voice channel specified in <see cref="vc"/>.
	/// </summary>
	/// <returns>Returns a <see cref="bool"/> specifying wether a disconnection occured (false means there was no connection to begin with)</returns>
	public async Task<OperationResult> DisconnectAsync(VoiceCommandContext vc)
	{
		if (_guildConnections.TryGetValue(vc.Channel.Guild.Id, out LavalinkGuildConnection? conn) || (conn = vc.GetGuildConnection()) is not null)
		{
			await conn.DisconnectAsync();
			_guildConnections.Remove(vc.Channel.Guild.Id);

			return Success();
		}

		// There was no connection.
		return Failure();
	}

	/// <summary>
	/// Checks if DreamJockey is connected to a voice channel in the specified guild.
	/// </summary>
	public bool IsConnected(ulong guildId) => _guildConnections.ContainsKey(guildId);

	/// <summary>
	/// Plays a track found with the specified <see cref="query"/>.
	/// </summary>
	public async Task<OperationResult<IEnumerable<LavalinkTrack>>> PlayAsync(VoiceCommandContext vc, string query)
	{
		LavalinkGuildConnection conn = await GetGuildConnectionAsync(vc);
		LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(query);

		if (loadResult.LoadResultType is not (LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches))
		{
			LavalinkTrack track = loadResult.Tracks.First();
			await conn.PlayAsync(track);

			return Success(loadResult.Tracks, $"Now playing `{track.Title}`.");
		}

		return Failure<IEnumerable<LavalinkTrack>>(null, $"Failed to find tracks for query `{query}`.");
	}

	/// <summary>
	/// Plays a track found with the specified <see cref="uri"/>.
	/// </summary>
	public async Task<OperationResult<LavalinkTrack>> PlayAsync(VoiceCommandContext vc, Uri uri)
	{
		LavalinkGuildConnection conn = await GetGuildConnectionAsync(vc);
		LavalinkLoadResult loadResult = await vc.Node.Rest.GetTracksAsync(uri);

		if (loadResult.LoadResultType is not (LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches))
		{
			LavalinkTrack track = loadResult.Tracks.First();
			await conn.PlayAsync(track);

			return Success(track, $"Now playing `{track.Title}`.");
		}

		return Failure<LavalinkTrack>(null, $"Failed to find tracks for query `{uri}`.");
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
			{ Result: { } conn } => await _StopAsync(conn)
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
			{ Result: { } conn } => await _PauseAsync(conn)
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

	private async Task<LavalinkGuildConnection> GetGuildConnectionAsync(VoiceCommandContext vc)
	{
		if (_guildConnections.TryGetValue(vc.Channel.Guild.Id, out LavalinkGuildConnection? conn))
		{
			return conn;
		}

		return await ConnectInternalAsync(vc);
	}

	private async Task<OperationResult<LavalinkGuildConnection>> GetContextGuildConnectionAsync(VoiceCommandContext vc, bool createIfNotExists = true)
	{
		if (vc.GetGuildConnection() is { } existing)
		{
			return Success(existing);
		}

		if (createIfNotExists && await ConnectInternalAsync(vc) is { } created)
		{
			return Success(created);
		}

		return Failure<LavalinkGuildConnection>(null, "Sorry, I'm currently not in any voice channel.");
	}
}