using Barotrauma.Networking;

namespace Neurotrauma
{
    // Clientside code ONLY!
    public partial class NeurotraumaInit
    {
        public void InitClientOnly()
        {
            ConfigurationMenu.AddConfigToPauseMenu();
            DynamicItems.InitDynamicItemsClient();

            LuaCsSetup.Instance.Networking.Receive("NT.ConfigUpdate", (object[] args) =>
            {
                IReadMessage msg = (IReadMessage)args[0];
                NTConfig.ReceiveConfig(msg);
            });
        }
    }
}
