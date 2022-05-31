using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace YumeChan.DreamJockey.Services;

/// <summary>
/// Provides q service for queuing abilities in music playback.
/// </summary>
public class MusicQueueService
{
	private readonly Dictionary<ulong, Queue<LavalinkTrack>> _musicQueues = new();
	
	/// <summary>
	/// Attemps to get the music queue for a specified guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="create">If true, creates a new queue if one does not exist.</param>
	/// <returns>The music queue.</returns>
	public Queue<LavalinkTrack>? GetMusicQueue(ulong guildId, bool create = false)
	{
		// Gets or creates the music queue for the specified guild.
		if (!_musicQueues.TryGetValue(guildId, out Queue<LavalinkTrack>? queue) && create)
		{
			queue = new();
			_musicQueues.Add(guildId, queue);
		}

		return queue;
	}
	
	/// <summary>
	/// Clears the music queue for a specified guild.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	public void ClearMusicQueue(ulong guildId)
	{
		if (_musicQueues.ContainsKey(guildId))
		{
			_musicQueues[guildId].Clear();
		}
	}

	/// <summary>
	/// Hooks the Lavalink events of a guild to its music queue, to ensure queuing functionality.
	/// </summary>
	/// <param name="vc">The voice command context.</param>
	/// <returns>The music queue.</returns>
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
				await conn.PlayAsync(queue.Dequeue());
			}
		}
	}
}