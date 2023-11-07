using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Interaction;
using SteamKit2;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Composition;

namespace ASFItemDropManager
{
    [Export(typeof(IPlugin))]
    public sealed class ASFItemDropManager : IBotSteamClient, IBotCommand2
    {
        private static ConcurrentDictionary<Bot, ItemDropHandler> ItemDropHandlers { get; } = new();
        public string Name => "ASF Item Dropper";
        public Version Version => typeof(ASFItemDropManager).Assembly.GetName().Version ?? new Version("0");

        public Task OnLoaded()
        {
            ASF.ArchiLogger.LogGenericInfo($"ASF Item Dropper Plugin by KSARDAS2K15 (chatgpt)");
            Directory.CreateDirectory(Path.Join("plugins", "ASFItemDropper", "droplogs"));
            return Task.CompletedTask;
        }

        public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0)
        {
            if (!Enum.IsDefined(access))
            {
                throw new InvalidEnumArgumentException(nameof(access), (int)access, typeof(EAccess));
            }

            switch (args[0].ToUpperInvariant())
            {
                case "IDROP" when args.Length == 4 && access >= EAccess.Master:
                    return await CheckItem(args[1], args[2], Utilities.GetArgsAsText(args, 3, ","), true).ConfigureAwait(false);
                case "IDROP" when args.Length == 3 && access >= EAccess.Master:
                    return await CheckItem(bot, args[1], Utilities.GetArgsAsText(args, 2, ","), true).ConfigureAwait(false);
                default:
                    return null;
            }
        }

		public Task OnBotSteamCallbacksInit(Bot bot, CallbackManager callbackManager) => Task.CompletedTask;

		public Task<IReadOnlyCollection<ClientMsgHandler>?> OnBotSteamHandlersInit(Bot bot)
		{
			var handler = new ItemDropHandler();
			ItemDropHandlers.TryAdd(bot, handler);
			return Task.FromResult<IReadOnlyCollection<ClientMsgHandler>?>(new HashSet<ClientMsgHandler> { handler });
		}

		private static async Task<string?> CheckItem(Bot bot, string appid, string itemdefId, bool longoutput)
		{
			if (!uint.TryParse(appid, out uint appId) || !uint.TryParse(itemdefId, out uint itemdefid))
			{
				return bot.Commands.FormatBotResponse(string.Format(Strings.ErrorIsInvalid, nameof(appId)));
			}
			if (!ItemDropHandlers.TryGetValue(bot, out ItemDropHandler? handler))
			{
				return bot.Commands.FormatBotResponse(string.Format(Strings.ErrorIsEmpty, nameof(ItemDropHandlers)));
			}
			return bot.Commands.FormatBotResponse(await Task.Run<string>(() => handler.checkTime(appId, itemdefid, bot, longoutput)).ConfigureAwait(false));
		}

private static async Task<string?> CheckItem(string botNames, string appid, string itemdefId, bool longoutput)
{
    var bots = Bot.GetBots(botNames);
    if (bots == null || bots.Count == 0)
    {
        return Commands.FormatStaticResponse(string.Format(Strings.BotNotFound, botNames));
    }

    var responses = new List<string?>();
    foreach (var bot in bots)
    {
        if (bot == null || !bot.IsConnectedAndLoggedOn)
        {
            responses.Add($"Bot {bot.BotName} is not connected or does not exist.");
            continue;
        }

        var result = await CheckItem(bot, appid, itemdefId, longoutput);
        if (!string.IsNullOrEmpty(result))
        {
            responses.Add(result);
        }
        await Task.Delay(500); // Задержка в 0.5 секунду
    }

    return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : "No Results";
}


    }
}
