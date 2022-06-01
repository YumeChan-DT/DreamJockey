using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace YumeChan.DreamJockey.Services;

/// <summary>
/// Provides a service for queuing abilities in music playback.
/// </summary>
public class MusicQueueService
{
	private readonly Dictionary<ulong, Queue<LavalinkTrack>> _musicQueues = new();
	private readonly Dictionary<ulong, DiscordChannel> _statusChannels = new();

	/// <summary>
	/// Attempts to get the music queue for a specified guild.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <param name="create">If true, creates a new queue if one does not exist.</param>
	/// <returns>The music queue.</returns>
	public Queue<LavalinkTrack>? GetMusicQueue(VoiceCommandContext vc, bool create = false)
	{
		bool exists = _musicQueues.TryGetValue(vc.Context.Guild.Id, out Queue<LavalinkTrack>? queue);
		
		// Gets or creates the music queue for the specified guild.
		if (!exists && create)
		{
			queue = new();
			_musicQueues.Add(vc.Context.Guild.Id, queue);
			_statusChannels.Add(vc.Context.Guild.Id, vc.Context.Channel);
			HookLavalinkEvents(vc);
			
			// If the lavalink connection exists, hook leave events to properly clear the queue.
			if (vc.GetGuildConnection() is { } conn)
			{
				conn.DiscordWebSocketClosed += OnLavalinkConnectionClosed;
			}
		}

		return queue;
	}
	
	public Queue<LavalinkTrack>? GetMusicQueue(ulong guildId) => _musicQueues.TryGetValue(guildId, out Queue<LavalinkTrack>? queue) ? queue : null;

	/// <summary>
	/// Clears the music queue for a specified guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	public void ClearMusicQueue(ulong guildId)
	{
		_musicQueues.Remove(guildId);
		_statusChannels.Remove(guildId);
	}

	/// <summary>
	/// Hooks the Lavalink events of a guild to its music queue, to ensure queuing functionality.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <exception cref="InvalidOperationException">Thrown if a voice context connection is not established.</exception>
	public void HookLavalinkEvents(VoiceCommandContext vc)
	{
		if (vc.GetGuildConnection() is not { } conn)
		{
			throw new InvalidOperationException("No voice context connection established.");
		}

		// Hook the events.
		conn.PlaybackFinished += OnTrackFinish;
	}
	
	/// <summary>
	/// Unhooks the Lavalink events of a guild from its music queue.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <exception cref="InvalidOperationException">Thrown if a voice context connection is not established.</exception>
	public void UnhookLavalinkEvents(VoiceCommandContext vc)
	{
		if (vc.GetGuildConnection() is not { } conn)
		{
			throw new InvalidOperationException("No voice context connection established.");
		}

		// Unhook the events.
		conn.PlaybackFinished -= OnTrackFinish;
	}
	
	/// <summary>
	/// Called when a track finishes playing.
	/// </summary>
	private async Task OnTrackFinish(LavalinkGuildConnection conn, TrackFinishEventArgs e)
	{
		// Get the music queue for the connection's guild.
		if (GetMusicQueue(conn.Guild.Id) is { } queue)
		{
			// If there are no more tracks in the queue, stop the connection.
			if (queue is { Count: 0 })
			{
				await conn.StopAsync();
			}
			// Otherwise, if the track finished in a natural way, play the next one.
			else if (e.Reason is TrackEndReason.Finished)
			{
				LavalinkTrack track = queue.Dequeue();
				await conn.PlayAsync(track);
				
				// Announce the track in the designated music status channel.
				if (_statusChannels.TryGetValue(conn.Guild.Id, out DiscordChannel? channel))
				{
					await channel.SendMessageAsync($"Now playing `{track.Title}`.");
				}
			}
		}
	}
	
	/// <summary>
	/// Trigerred when a Lavalink connection is closed (WebSocket closed).
	/// </summary>
	/// <param name="conn">The Lavalink connection.</param>
	/// <param name="e">The event arguments.</param>
	private Task OnLavalinkConnectionClosed(LavalinkGuildConnection conn, WebSocketCloseEventArgs e)
	{
		ClearMusicQueue(conn.Guild.Id);
		return Task.CompletedTask;
	}
}