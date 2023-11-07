using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using SteamKit2;
using SteamKit2.Internal;
using System.Collections.Concurrent;

namespace ASFItemDropManager
{
    public sealed class ItemDropHandler : ClientMsgHandler
    {
        private SteamUnifiedMessages.UnifiedService<IInventory>? _inventoryService;
        private ConcurrentDictionary<ulong, StoredResponse> Responses = new ConcurrentDictionary<ulong, StoredResponse>();

        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg == null)
            {
                ASF.ArchiLogger.LogNullError(nameof(packetMsg));
                return;
            }
        }

        internal async Task<string> checkTime(uint appid, uint itemdefid, Bot bot, bool longoutput)
        {
            var steamUnifiedMessages = Client.GetHandler<SteamUnifiedMessages>();
            if (steamUnifiedMessages == null)
            {
                bot.ArchiLogger.LogNullError(nameof(steamUnifiedMessages));
                return "SteamUnifiedMessages Error";
            }

            CInventory_ConsumePlaytime_Request playtimeRequest = new CInventory_ConsumePlaytime_Request { appid = appid, itemdefid = itemdefid };
            _inventoryService = steamUnifiedMessages.CreateService<IInventory>();
            var playtimeResponse = await _inventoryService.SendMessage(x => x.ConsumePlaytime(playtimeRequest));
            var resultGamesPlayed = playtimeResponse.GetDeserializedResponse<CInventory_Response>();

            if (resultGamesPlayed == null) 
            {
                bot.ArchiLogger.LogNullError("resultGamesPlayed");
                return "No item drop detected.";
            }

            if (resultGamesPlayed.item_json != "[]")
            {
                return $"Item drop detected! Item ID: {itemdefid}";
            }
            else
            {
                return "No item drop detected.";
            }
        }
    }
}
